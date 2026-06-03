using System.IO;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;

namespace DormitoryManagement.WPF.Tests;

public sealed class RegisterWpfResourceTests
{
    [Fact]
    public void Compiled_resources_include_register_view_and_resource_dictionary()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/auth/registerview.baml", keys);
        Assert.Contains("resources/registerpage.baml", keys);
    }

    [Fact]
    public void Register_page_resources_define_expected_font_and_color_tokens()
    {
        var document = LoadRegisterPageResourceDocument();
        var fontFamilies = document.Root!
            .Elements(WpfNamespace + "FontFamily")
            .ToDictionary(
                element => element.Attribute(XamlNamespace + "Key")?.Value ?? string.Empty,
                element => element.Value.Trim(),
                StringComparer.Ordinal);

        Assert.Equal("pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/PlusJakartaSans/#Plus Jakarta Sans", fontFamilies["RegisterPageHeadlineFontFamily"]);
        Assert.Equal("pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/Inter/#Inter", fontFamilies["RegisterPageBodyFontFamily"]);
        AssertResourceValue(document, "Color", "RegisterPageColorBackground", "#F8F9FF");
        AssertResourceValue(document, "Color", "RegisterPageColorPrimary", "#AB3500");
        AssertResourceValue(document, "Color", "RegisterPageColorPrimaryContainer", "#FF6B35");
        AssertResourceValue(document, "CornerRadius", "RegisterPageControlCornerRadius", "8");
        AssertResourceValue(document, "CornerRadius", "RegisterPageCardCornerRadius", "12");
    }

    [Fact]
    public void Register_view_xaml_preserves_reference_field_order_and_consent_row()
    {
        var xaml = LoadRegisterViewDocument().ToString(SaveOptions.DisableFormatting);

        AssertStringOrder(
            xaml,
            "Họ và tên",
            "Mã số sinh viên",
            "Tên đăng nhập",
            "Ngày sinh",
            "Giới tính",
            "Số điện thoại",
            "Email",
            "Gửi mã",
            "Mật khẩu",
            "Xác nhận mật khẩu",
            "Tôi đồng ý với",
            "Đăng ký ngay",
            "Đã có tài khoản?",
            "Đăng nhập");
    }

    [Fact]
    public void Register_view_xaml_declares_left_hero_right_form_and_reference_copy()
    {
        var xaml = LoadRegisterViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("Grid.Column=\"0\"", xaml);
        Assert.Contains("Grid.Column=\"1\"", xaml);
        Assert.Contains("register-hero.png", xaml);
        Assert.Contains("Chào mừng bạn đến với không gian sống hiện đại", xaml);
        Assert.Contains("Điền thông tin dưới đây để tạo tài khoản mới.", xaml);
        Assert.DoesNotContain("Create Student Account", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Send verification code", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Register_hero_asset_exists_for_repeatable_review()
    {
        var repoRoot = FindRepositoryRoot();
        var assetPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Images", "Register", "register-hero.png");

        Assert.True(File.Exists(assetPath));
    }

    private static HashSet<string> GetCompiledResourceKeys()
    {
        using var stream = Assembly.Load("DormitoryManagement.WPF").GetManifestResourceStream("DormitoryManagement.WPF.g.resources");
        Assert.NotNull(stream);
        using var reader = new ResourceReader(stream!);
        return reader.Cast<System.Collections.DictionaryEntry>()
            .Select(entry => Assert.IsType<string>(entry.Key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static XDocument LoadRegisterPageResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var resourcePath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "RegisterPage.xaml");

        Assert.True(File.Exists(resourcePath));
        return XDocument.Load(resourcePath);
    }

    private static XDocument LoadRegisterViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Auth", "RegisterView.xaml");

        Assert.True(File.Exists(viewPath));
        return XDocument.Load(viewPath);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var projectFile = Path.Combine(current.FullName, "src", "DormitoryManagement.WPF", "DormitoryManagement.WPF.csproj");
            if (File.Exists(projectFile))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new Xunit.Sdk.XunitException("Could not locate repository root from test output directory.");
    }

    private static void AssertResourceValue(XDocument document, string localName, string key, string expectedValue)
    {
        var element = document.Root!
            .Elements()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name.LocalName, localName, StringComparison.Ordinal)
                && string.Equals(candidate.Attribute(XamlNamespace + "Key")?.Value, key, StringComparison.Ordinal));

        Assert.NotNull(element);
        Assert.Equal(expectedValue, element!.Value.Trim());
    }

    private static void AssertStringOrder(string text, params string[] orderedValues)
    {
        var lastIndex = -1;
        foreach (var value in orderedValues)
        {
            var index = text.IndexOf(value, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Expected to find '{value}' in XAML content.");
            Assert.True(index > lastIndex, $"Expected '{value}' to appear after the previous contract string.");
            lastIndex = index;
        }
    }

    private static readonly XNamespace WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
}
