using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WinformsMVP.Core.Views;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// IViewMappingRegister の拡張メソッド。
    /// 自動スキャン登録など、便利な機能を提供します。
    /// </summary>
    public static class ViewMappingRegisterExtensions
    {
        /// <summary>
        /// 指定されたアセンブリから View インターフェースと実装 Form のマッピングを自動的にスキャンして登録します。
        /// </summary>
        /// <param name="register">ViewMappingRegister インスタンス</param>
        /// <param name="assembly">スキャン対象のアセンブリ</param>
        /// <param name="allowOverride">既存のマッピングを上書きするかどうか（デフォルト: false）</param>
        /// <returns>登録された View の数</returns>
        /// <remarks>
        /// <para>
        /// このメソッドは以下の条件を満たす型を自動登録します：
        /// <list type="bullet">
        /// <item>Form を継承している</item>
        /// <item>IWindowView または IViewBase を実装している</item>
        /// <item>パラメータなしのパブリックコンストラクタを持つ</item>
        /// <item>抽象クラスでない</item>
        /// </list>
        /// </para>
        /// <para>
        /// 例: NavigatorDemoForm が INavigatorDemoView を実装している場合、
        /// Register&lt;INavigatorDemoView, NavigatorDemoForm&gt;() が自動的に呼び出されます。
        /// </para>
        /// </remarks>
        public static int RegisterFromAssembly(
            this IViewMappingRegister register,
            Assembly assembly,
            bool allowOverride = false)
        {
            if (register == null)
                throw new ArgumentNullException(nameof(register));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            int registeredCount = 0;

            // アセンブリ内のすべての型を取得
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 一部の型が読み込めない場合でも、読み込めた型だけ処理
                types = ex.Types.Where(t => t != null).ToArray();
            }

            // Form を継承しているクラスを検索
            var formTypes = types
                .Where(t => t.IsClass && !t.IsAbstract && typeof(Form).IsAssignableFrom(t))
                .ToList();

            foreach (var formType in formTypes)
            {
                // パラメータなしのパブリックコンストラクタをチェック
                var hasParameterlessConstructor = formType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null) != null;

                if (!hasParameterlessConstructor)
                {
                    // パラメータなしコンストラクタがない場合はスキップ（警告なし）
                    continue;
                }

                // IWindowView または IViewBase を実装しているインターフェースを検索
                var viewInterface = formType.GetInterfaces()
                    .FirstOrDefault(i =>
                        i != typeof(IWindowView) &&
                        i != typeof(IViewBase) &&
                        (typeof(IWindowView).IsAssignableFrom(i) || typeof(IViewBase).IsAssignableFrom(i)));

                if (viewInterface != null)
                {
                    try
                    {
                        // リフレクションで Register<TInterface, TImplementation>() を呼び出し
                        var registerMethod = typeof(IViewMappingRegister)
                            .GetMethods()
                            .First(m => m.Name == "Register" &&
                                       m.IsGenericMethodDefinition &&
                                       m.GetGenericArguments().Length == 2)
                            .MakeGenericMethod(viewInterface, formType);

                        registerMethod.Invoke(register, new object[] { allowOverride });
                        registeredCount++;
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is InvalidOperationException && !allowOverride)
                        {
                            // 既に登録されている場合はスキップ（allowOverride = false のとき）
                            continue;
                        }
                        throw;
                    }
                }
            }

            return registeredCount;
        }

        /// <summary>
        /// 現在のアプリケーションドメインに読み込まれているすべてのアセンブリから
        /// View インターフェースと実装 Form のマッピングを自動的にスキャンして登録します。
        /// </summary>
        /// <param name="register">ViewMappingRegister インスタンス</param>
        /// <param name="allowOverride">既存のマッピングを上書きするかどうか（デフォルト: false）</param>
        /// <param name="excludeSystemAssemblies">システムアセンブリを除外するかどうか（デフォルト: true）</param>
        /// <returns>登録された View の数</returns>
        /// <remarks>
        /// <para>
        /// このメソッドは、アプリケーションドメインに読み込まれているすべてのアセンブリを対象とします。
        /// 通常は特定のアセンブリを指定する RegisterFromAssembly() の使用を推奨します。
        /// </para>
        /// </remarks>
        public static int RegisterFromLoadedAssemblies(
            this IViewMappingRegister register,
            bool allowOverride = false,
            bool excludeSystemAssemblies = true)
        {
            if (register == null)
                throw new ArgumentNullException(nameof(register));

            int totalRegistered = 0;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // システムアセンブリを除外
                if (excludeSystemAssemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    if (assemblyName.StartsWith("System.") ||
                        assemblyName.StartsWith("Microsoft.") ||
                        assemblyName == "mscorlib" ||
                        assemblyName == "netstandard")
                    {
                        continue;
                    }
                }

                try
                {
                    totalRegistered += RegisterFromAssembly(register, assembly, allowOverride);
                }
                catch
                {
                    // アセンブリスキャンに失敗しても継続
                    continue;
                }
            }

            return totalRegistered;
        }

        /// <summary>
        /// 指定された名前空間内の View インターフェースと実装 Form のマッピングを自動的にスキャンして登録します。
        /// </summary>
        /// <param name="register">ViewMappingRegister インスタンス</param>
        /// <param name="assembly">スキャン対象のアセンブリ</param>
        /// <param name="namespacePrefix">名前空間プレフィックス（例: "MyApp.Views"）</param>
        /// <param name="allowOverride">既存のマッピングを上書きするかどうか（デフォルト: false）</param>
        /// <returns>登録された View の数</returns>
        public static int RegisterFromNamespace(
            this IViewMappingRegister register,
            Assembly assembly,
            string namespacePrefix,
            bool allowOverride = false)
        {
            if (register == null)
                throw new ArgumentNullException(nameof(register));
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrEmpty(namespacePrefix))
                throw new ArgumentException("Namespace prefix cannot be null or empty", nameof(namespacePrefix));

            int registeredCount = 0;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray();
            }

            // 指定された名前空間のFormクラスのみをフィルタ
            var formTypes = types
                .Where(t => t.IsClass &&
                           !t.IsAbstract &&
                           typeof(Form).IsAssignableFrom(t) &&
                           t.Namespace != null &&
                           t.Namespace.StartsWith(namespacePrefix))
                .ToList();

            foreach (var formType in formTypes)
            {
                var hasParameterlessConstructor = formType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null) != null;

                if (!hasParameterlessConstructor)
                    continue;

                var viewInterface = formType.GetInterfaces()
                    .FirstOrDefault(i =>
                        i != typeof(IWindowView) &&
                        i != typeof(IViewBase) &&
                        (typeof(IWindowView).IsAssignableFrom(i) || typeof(IViewBase).IsAssignableFrom(i)));

                if (viewInterface != null)
                {
                    try
                    {
                        var registerMethod = typeof(IViewMappingRegister)
                            .GetMethods()
                            .First(m => m.Name == "Register" &&
                                       m.IsGenericMethodDefinition &&
                                       m.GetGenericArguments().Length == 2)
                            .MakeGenericMethod(viewInterface, formType);

                        registerMethod.Invoke(register, new object[] { allowOverride });
                        registeredCount++;
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException is InvalidOperationException && !allowOverride)
                            continue;
                        throw;
                    }
                }
            }

            return registeredCount;
        }
    }
}
