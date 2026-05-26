
using System.Windows.Controls;

using CoPilotStatusExtension.GitHubApiModels;
using CoPilotStatusExtension.Models;
using CoPilotStatusExtension.ViewModels;

//-----------------------------------------------------------------------------------------------------------------------------------------
namespace CoPilotStatusExtension.Views;

//-----------------------------------------------------------------------------------------------------------------------------------------
public partial class GitHubStatusBarControl : UserControl
{
	//-----------------------------------------------------------------------------------------------------------------
	#region Fields

	private readonly GitHubStatusBarViewModel	_viewModel		= new();
	private readonly IThemeManager				_themeManager;

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Construction

	public GitHubStatusBarControl()
		: this(new ThemeManager()) { }

	/// <summary>
	/// This constructor is for unit testing only/mainly.
	/// It allows injecting a mock <see cref="IThemeManager"/> to verify that Initialize and Cleanup are called appropriately.
	/// </summary>
	/// <param name="themeManager">The theme manager to use for this control.</param>
	internal GitHubStatusBarControl(IThemeManager themeManager)
	{
		_themeManager = themeManager;
		InitializeComponent();
		DataContext = _viewModel;
		_themeManager.Initialize();
		Unloaded += (_, _) => _themeManager.Cleanup();
	}

	#endregion Construction

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit)
		=> _viewModel.SetData(copilotUserInfo, billingUsage, personalQuota, apiRateLimit);

	#endregion Methods
}
