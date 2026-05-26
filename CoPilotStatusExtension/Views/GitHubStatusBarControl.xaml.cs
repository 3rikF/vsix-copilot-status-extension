
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

	private readonly GitHubStatusBarViewModel _viewModel = new();

	#endregion Fields

	//-----------------------------------------------------------------------------------------------------------------
	#region Construction

	public GitHubStatusBarControl()
	{
		InitializeComponent();
		DataContext = _viewModel;
	}

	#endregion Construction

	//-----------------------------------------------------------------------------------------------------------------
	#region Methods

	public void SetData(CopilotUserInfo? copilotUserInfo, CopilotBillingUsage? billingUsage, CopilotQuotaResponse? personalQuota, RateLimitInfo? apiRateLimit)
		=> _viewModel.SetData(copilotUserInfo, billingUsage, personalQuota, apiRateLimit);

	#endregion Methods
}
