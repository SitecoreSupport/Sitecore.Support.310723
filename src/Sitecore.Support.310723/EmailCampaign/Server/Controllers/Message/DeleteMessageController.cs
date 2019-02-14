using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Server.Contexts;
using Sitecore.EmailCampaign.Server.Filters;
using Sitecore.EmailCampaign.Server.Responses;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Data;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Web.Http;
using System;
using System.Web.Http;

namespace Sitecore.EmailCampaign.Server.Controllers.Message
{
  [ServicesController("EXM.DeleteMessage")]
  public class DeleteMessageController : ServicesApiController
  {
    private readonly ItemUtilExt util;

    private readonly EcmDataProvider _dataProvider;

    private readonly ILogger _logger;

    public DeleteMessageController(ItemUtilExt util, EcmDataProvider dataProvider, ILogger logger)
    {
      Assert.ArgumentNotNull(util, "util");
      Assert.ArgumentNotNull(dataProvider, "dataProvider");
      Assert.ArgumentNotNull(logger, "logger");
      this.util = util;
      this._dataProvider = dataProvider;
      this._logger = logger;
    }

    [ActionName("DefaultAction")]
    public Response Process(StringContext data)
    {
      Assert.ArgumentNotNull(data, "requestArgs");
      Assert.IsNotNullOrEmpty(data.Value, "Could not get message id from the string context for requestArgs:{0}", new object[]
      {
                data
      });
      MessageBarResponse messageBarResponse = new MessageBarResponse();
      try
      {
        Item item = this.util.GetItem(data.Value);
        if (item == null)
        {
          messageBarResponse.Type = "error";
          messageBarResponse.Message = "Message could not be found.";
          return messageBarResponse;
        }
        messageBarResponse.Type = "notification";
        messageBarResponse.Message = "The message " + item.DisplayName + " has been deleted.";
        item.Delete();
        this._dataProvider.DeleteCampaign(item.ID.Guid);
      }
      catch (Exception ex)
      {
        this._logger.LogError(ex.Message, ex);
        messageBarResponse.Error = true;
      }
      return messageBarResponse;
    }
  }
}
