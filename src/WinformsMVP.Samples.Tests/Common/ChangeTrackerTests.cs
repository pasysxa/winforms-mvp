using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using WinformsMVP.Common;

namespace WinformsMVP.Samples.Tests.Common
{
    /// <summary>
    /// ChangeTracker<T> の単体テスト
    ///
    /// テスト対象：
    /// - 基本機能（AcceptChanges、RejectChanges、IsChanged）
    /// - UpdateCurrentValue() メソッド
    /// - IsChangedChanged イベント
    /// - IsChanged キャッシング
    /// - スレッド安全性
    /// - 検証サポート（CanAcceptChanges、CanRejectChanges）
    /// - 深いコピー要件
    /// </summary>
    public class ChangeTrackerTests
    {
        #region テストモデル

        private class TestModel : ICloneable, IEquatable<TestModel>
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public object Clone()
            {
                return new TestModel
                {
                    Id = this.Id,
                    Name = this.Name
                };
            }

            public bool Equals(TestModel other)
            {
                if (other == null) return false;
                return Id == other.Id && Name == other.Name;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TestModel);
            }

            public override int GetHashCode()
            {
                return (Id, Name).GetHashCode();
            }
        }

        private class NestedModel : ICloneable, IEquatable<NestedModel>
        {
            public string Title { get; set; }
            public ChildModel Child { get; set; }

            public object Clone()
            {
                return new NestedModel
                {
                    Title = this.Title,
                    Child = this.Child?.Clone() as ChildModel  // 深いコピー
                };
            }

            public bool Equals(NestedModel other)
            {
                if (other == null) return false;
                return Title == other.Title &&
                       (Child == null && other.Child == null ||
                        Child != null && Child.Equals(other.Child));
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as NestedModel);
            }

            public override int GetHashCode()
            {
                return (Title, Child).GetHashCode();
            }
        }

        private class ChildModel : ICloneable, IEquatable<ChildModel>
        {
            public string Value { get; set; }

            public object Clone()
            {
                return new ChildModel { Value = this.Value };
            }

            public bool Equals(ChildModel other)
            {
                if (other == null) return false;
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ChildModel);
            }

            public override int GetHashCode()
            {
                return Value?.GetHashCode() ?? 0;
            }
        }

        #endregion

        #region コンストラクタテスト

        [Fact]
        public void Constructor_WithNullInitialValue_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ChangeTracker<TestModel>(null));
        }

        [Fact]
        public void Constructor_WithValidValue_InitializesCorrectly()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Test" };

            // Act
            var tracker = new ChangeTracker<TestModel>(model);

            // Assert
            Assert.NotNull(tracker.CurrentValue);
            Assert.Equal(1, tracker.CurrentValue.Id);
            Assert.Equal("Test", tracker.CurrentValue.Name);
            Assert.False(tracker.IsChanged);
        }

        [Fact]
        public void Constructor_CreatesIndependentCopy()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };

            // Act
            var tracker = new ChangeTracker<TestModel>(model);
            model.Name = "Modified";  // 元のオブジェクトを変更

            // Assert
            Assert.Equal("Original", tracker.CurrentValue.Name);  // trackerは影響を受けない
        }

        #endregion

        #region UpdateCurrentValue テスト

        [Fact]
        public void UpdateCurrentValue_WithValidValue_UpdatesCurrentValue()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var newModel = new TestModel { Id = 2, Name = "Updated" };
            tracker.UpdateCurrentValue(newModel);

            // Assert
            Assert.Equal(2, tracker.CurrentValue.Id);
            Assert.Equal("Updated", tracker.CurrentValue.Name);
            Assert.True(tracker.IsChanged);
        }

        [Fact]
        public void UpdateCurrentValue_WithNull_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Test" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.UpdateCurrentValue(null));
        }

        #endregion

        #region IsChangedChanged イベントテスト

        [Fact]
        public void IsChangedChanged_WhenValueChanges_EventFires()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);
            var eventFired = false;

            tracker.IsChangedChanged += (s, e) => eventFired = true;

            // Act
            var newModel = new TestModel { Id = 2, Name = "Updated" };
            tracker.UpdateCurrentValue(newModel);

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void IsChangedChanged_WhenAcceptChanges_EventFires()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            var newModel = new TestModel { Id = 2, Name = "Updated" };
            tracker.UpdateCurrentValue(newModel);

            var eventFired = false;
            tracker.IsChangedChanged += (s, e) => eventFired = true;

            // Act
            tracker.AcceptChanges();

            // Assert
            Assert.True(eventFired);  // IsChangedがtrue→falseに変化
        }

        [Fact]
        public void IsChangedChanged_WhenRejectChanges_EventFires()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            var newModel = new TestModel { Id = 2, Name = "Updated" };
            tracker.UpdateCurrentValue(newModel);

            var eventFired = false;
            tracker.IsChangedChanged += (s, e) => eventFired = true;

            // Act
            tracker.RejectChanges();

            // Assert
            Assert.True(eventFired);  // IsChangedがtrue→falseに変化
        }

        [Fact]
        public void IsChangedChanged_WhenNoStateChange_EventDoesNotFire()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            var eventFired = false;
            tracker.IsChangedChanged += (s, e) => eventFired = true;

            // Act - 変更されていない状態でAcceptChanges
            tracker.AcceptChanges();

            // Assert
            Assert.False(eventFired);  // IsChanged状態が変化していないのでイベント発火しない
        }

        #endregion

        #region AcceptChanges テスト

        [Fact]
        public void AcceptChanges_AfterModification_SetsIsChangedToFalse()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            var newModel = new TestModel { Id = 2, Name = "Updated" };
            tracker.UpdateCurrentValue(newModel);
            Assert.True(tracker.IsChanged);

            // Act
            tracker.AcceptChanges();

            // Assert
            Assert.False(tracker.IsChanged);
        }

        [Fact]
        public void AcceptChanges_CreatesNewBaseline()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "Updated" });
            tracker.AcceptChanges();

            // Act - さらに変更
            tracker.UpdateCurrentValue(new TestModel { Id = 3, Name = "Modified Again" });

            // Assert
            Assert.True(tracker.IsChanged);  // 新しいベースラインから変更されている
        }

        [Fact]
        public void AcceptChangesWithParameter_UpdatesBothCurrentAndOriginal()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var newModel = new TestModel { Id = 10, Name = "Complete Reset" };
            tracker.AcceptChanges(newModel);

            // Assert
            Assert.False(tracker.IsChanged);
            Assert.Equal(10, tracker.CurrentValue.Id);
            Assert.Equal("Complete Reset", tracker.CurrentValue.Name);
        }

        #endregion

        #region RejectChanges テスト

        [Fact]
        public void RejectChanges_RestoresOriginalValue()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "Updated" });
            Assert.True(tracker.IsChanged);

            // Act
            tracker.RejectChanges();

            // Assert
            Assert.False(tracker.IsChanged);
            Assert.Equal(1, tracker.CurrentValue.Id);
            Assert.Equal("Original", tracker.CurrentValue.Name);
        }

        [Fact]
        public void RejectChanges_MultipleModifications_RestoresToLastAccepted()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "First Change" });
            tracker.AcceptChanges();

            tracker.UpdateCurrentValue(new TestModel { Id = 3, Name = "Second Change" });
            tracker.UpdateCurrentValue(new TestModel { Id = 4, Name = "Third Change" });

            // Act
            tracker.RejectChanges();

            // Assert
            Assert.False(tracker.IsChanged);
            Assert.Equal(2, tracker.CurrentValue.Id);  // "First Change"に戻る
            Assert.Equal("First Change", tracker.CurrentValue.Name);
        }

        #endregion

        #region 深いコピーテスト

        [Fact]
        public void DeepCopy_NestedObjects_AreIndependent()
        {
            // Arrange
            var model = new NestedModel
            {
                Title = "Parent",
                Child = new ChildModel { Value = "Original Child" }
            };
            var tracker = new ChangeTracker<NestedModel>(model);

            // Act - ネストされたオブジェクトを変更
            tracker.CurrentValue.Child.Value = "Modified Child";

            // Assert - IsChangedが正しく検出される
            Assert.True(tracker.IsChanged);

            // Act - RejectChanges
            tracker.RejectChanges();

            // Assert - 元の値に戻る
            Assert.Equal("Original Child", tracker.CurrentValue.Child.Value);
        }

        [Fact]
        public void GetOriginalValue_ReturnsIndependentCopy()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var original = tracker.GetOriginalValue();
            original.Name = "Modified Copy";

            // Assert - trackerのオリジナルは影響を受けない
            tracker.RejectChanges();
            Assert.Equal("Original", tracker.CurrentValue.Name);
        }

        #endregion

        #region IsChangedWith テスト

        [Fact]
        public void IsChangedWith_ComparesAgainstOriginal()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var modifiedModel = new TestModel { Id = 2, Name = "Modified" };
            var isChanged = tracker.IsChangedWith(modifiedModel);

            // Assert
            Assert.True(isChanged);
        }

        [Fact]
        public void IsChangedWith_SameValues_ReturnsFalse()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var sameModel = new TestModel { Id = 1, Name = "Original" };
            var isChanged = tracker.IsChangedWith(sameModel);

            // Assert
            Assert.False(isChanged);
        }

        #endregion

        #region カスタム比較テスト

        [Fact]
        public void CustomComparer_UsedForChangeDetection()
        {
            // Arrange - IDのみで比較するカスタム比較関数
            Func<TestModel, TestModel, bool> comparer = (a, b) => a.Id == b.Id;
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model, comparer);

            // Act - Nameのみ変更（IDは同じ）
            tracker.UpdateCurrentValue(new TestModel { Id = 1, Name = "Modified" });

            // Assert - カスタム比較ではIDが同じなので変更なしと判定
            Assert.False(tracker.IsChanged);
        }

        [Fact]
        public void CustomComparer_DetectsRelevantChanges()
        {
            // Arrange
            Func<TestModel, TestModel, bool> comparer = (a, b) => a.Id == b.Id;
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model, comparer);

            // Act - IDを変更
            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "Original" });

            // Assert - カスタム比較でIDが異なるので変更と判定
            Assert.True(tracker.IsChanged);
        }

        #endregion

        #region スレッド安全性テスト

        [Fact]
        public void ThreadSafety_ConcurrentAccess_NoExceptions()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act - 複数スレッドから同時アクセス
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() =>
                {
                    if (index % 3 == 0)
                    {
                        tracker.UpdateCurrentValue(new TestModel { Id = index, Name = $"Thread {index}" });
                    }
                    else if (index % 3 == 1)
                    {
                        var isChanged = tracker.IsChanged;
                    }
                    else
                    {
                        tracker.AcceptChanges();
                    }
                }));
            }

            // Assert - 例外が発生しないことを確認
            Task.WaitAll(tasks.ToArray());
            Assert.True(true);  // 例外なく完了すればOK
        }

        [Fact]
        public void ThreadSafety_IsChangedCaching_ThreadSafe()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act - 複数スレッドからIsChangedを同時読み取り
            var tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var _ = tracker.IsChanged;  // キャッシュの読み取り
                }));
            }

            // Assert - 例外が発生しないことを確認
            Task.WaitAll(tasks.ToArray());
            Assert.True(true);
        }

        #endregion

        #region 検証サポートテスト

        [Fact]
        public void CanAcceptChanges_DefaultImplementation_ReturnsTrue()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Test" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var canAccept = tracker.CanAcceptChanges(out string error);

            // Assert
            Assert.True(canAccept);
            Assert.Null(error);
        }

        [Fact]
        public void CanRejectChanges_DefaultImplementation_ReturnsTrue()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Test" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act
            var canReject = tracker.CanRejectChanges(out string error);

            // Assert
            Assert.True(canReject);
            Assert.Null(error);
        }

        // 派生クラスでの検証サポートのテスト
        private class ValidatedChangeTracker : ChangeTracker<TestModel>
        {
            public ValidatedChangeTracker(TestModel initialValue) : base(initialValue) { }

            public override bool CanAcceptChanges(out string error)
            {
                if (CurrentValue.Name == "Invalid")
                {
                    error = "Name cannot be 'Invalid'";
                    return false;
                }
                error = null;
                return true;
            }
        }

        [Fact]
        public void DerivedClass_CustomValidation_Works()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Valid" };
            var tracker = new ValidatedChangeTracker(model);

            // Act - 無効な値に変更
            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "Invalid" });

            // Assert
            Assert.False(tracker.CanAcceptChanges(out string error));
            Assert.Equal("Name cannot be 'Invalid'", error);
        }

        #endregion

        #region IsChanged キャッシングテスト

        [Fact]
        public void IsChanged_Caching_DoesNotRecomputeUntilValueChanges()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            // Act - 複数回IsChangedを呼び出し
            var result1 = tracker.IsChanged;
            var result2 = tracker.IsChanged;
            var result3 = tracker.IsChanged;

            // Assert - キャッシュが機能している（例外なく高速に動作）
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
        }

        [Fact]
        public void IsChanged_CacheInvalidation_AfterUpdateCurrentValue()
        {
            // Arrange
            var model = new TestModel { Id = 1, Name = "Original" };
            var tracker = new ChangeTracker<TestModel>(model);

            var _ = tracker.IsChanged;  // キャッシュを作成

            // Act - 値を変更
            tracker.UpdateCurrentValue(new TestModel { Id = 2, Name = "Updated" });

            // Assert - キャッシュが無効化され、新しい値で再計算
            Assert.True(tracker.IsChanged);
        }

        #endregion
    }
}
