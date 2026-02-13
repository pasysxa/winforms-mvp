using System;
using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.Tests.Mocks
{
    /// <summary>
    /// IWindowNavigatorのモック実装（テスト用）。
    /// ウィンドウ表示を記録し、テストで検証可能にします。
    /// </summary>
    public class MockWindowNavigator : IWindowNavigator
    {
        // 記録用
        public int ShowWindowCalls { get; private set; }
        public int ShowModalCalls { get; private set; }
        public object LastPresenter { get; private set; }
        public object LastParameters { get; private set; }

        public void Clear()
        {
            ShowWindowCalls = 0;
            ShowModalCalls = 0;
            LastPresenter = null;
            LastParameters = null;
        }

        // Non-modal window methods
        public IWindowView ShowWindow<TPresenter, TResult>(
            TPresenter presenter,
            IWin32Window owner = null,
            Func<TPresenter, object> keySelector = null,
            Action<InteractionResult<TResult>> onClosed = null)
            where TPresenter : IPresenter
        {
            ShowWindowCalls++;
            LastPresenter = presenter;
            return null;  // テスト用mockなのでnullを返す
        }

        public IWindowView ShowWindow<TPresenter>(
            TPresenter presenter,
            IWin32Window owner = null,
            Func<TPresenter, object> keySelector = null)
            where TPresenter : IPresenter
        {
            ShowWindowCalls++;
            LastPresenter = presenter;
            return null;
        }

        public IWindowView ShowWindow<TPresenter, TParam, TResult>(
            TPresenter presenter,
            TParam parameters,
            IWin32Window owner = null,
            Func<TPresenter, object> keySelector = null,
            Action<InteractionResult<TResult>> onClosed = null)
            where TPresenter : IPresenter, IInitializable<TParam>
        {
            ShowWindowCalls++;
            LastPresenter = presenter;
            LastParameters = parameters;
            return null;
        }

        public IWindowView ShowWindow<TPresenter, TParam>(
            TPresenter presenter,
            TParam parameters,
            IWin32Window owner = null,
            Func<TPresenter, object> keySelector = null)
            where TPresenter : IPresenter, IInitializable<TParam>
        {
            ShowWindowCalls++;
            LastPresenter = presenter;
            LastParameters = parameters;
            return null;
        }

        // Modal window methods
        public InteractionResult ShowWindowAsModal<TPresenter>(
            TPresenter presenter,
            IWin32Window owner = null)
            where TPresenter : IPresenter
        {
            ShowModalCalls++;
            LastPresenter = presenter;
            return InteractionResult.Cancel();  // テスト用のデフォルト値
        }

        public InteractionResult<TResult> ShowWindowAsModal<TPresenter, TResult>(
            TPresenter presenter,
            IWin32Window owner = null)
            where TPresenter : IPresenter
        {
            ShowModalCalls++;
            LastPresenter = presenter;
            return InteractionResult<TResult>.Cancel();
        }

        public InteractionResult ShowWindowAsModal<TPresenter, TParam>(
            TPresenter presenter,
            TParam parameters,
            IWin32Window owner = null)
            where TPresenter : IPresenter, IInitializable<TParam>
        {
            ShowModalCalls++;
            LastPresenter = presenter;
            LastParameters = parameters;
            return InteractionResult.Cancel();
        }

        public InteractionResult<TResult> ShowWindowAsModal<TPresenter, TParam, TResult>(
            TPresenter presenter,
            TParam parameters,
            IWin32Window owner = null)
            where TPresenter : IPresenter, IInitializable<TParam>
        {
            ShowModalCalls++;
            LastPresenter = presenter;
            LastParameters = parameters;
            return InteractionResult<TResult>.Cancel();
        }
    }
}
