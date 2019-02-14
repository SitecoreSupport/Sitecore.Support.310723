using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Server.Contexts;
using Sitecore.EmailCampaign.Server.Responses;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Data;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Web.Http;
using System;
using System.Web.Http;
using Sitecore.Modules.EmailCampaign.Messages;
using Sitecore.Tasks;
using Sitecore.Modules.EmailCampaign.Factories;
using System.Data.SqlClient;
using System.Configuration;

namespace Sitecore.EmailCampaign.Server.Controllers.Message
{
  [ServicesController("EXM.SupportDeleteMessage")]
  public class SupportDeleteMessageController : ServicesApiController
  {
    private ItemUtilExt util;

    private readonly EcmDataProvider _dataProvider;

    private readonly ILogger _logger;

    private string _connectionString;

    public string ConnectionString
    {
      get
      {
        if (string.IsNullOrWhiteSpace("exm.master") && string.IsNullOrWhiteSpace(this._connectionString))
        {
          return null;
        }
        if (!string.IsNullOrWhiteSpace(this._connectionString))
        {
          return this._connectionString;
        }
        ConnectionStringSettings expr_3F = ConfigurationManager.ConnectionStrings["exm.master"];
        this._connectionString = ((expr_3F != null) ? expr_3F.ConnectionString : null);
        
        return this._connectionString;
      }
      internal set
      {
        this._connectionString = value;
      }
    }

    public SupportDeleteMessageController()
    {

    }
    public SupportDeleteMessageController(ItemUtilExt util, EcmDataProvider dataProvider, ILogger logger)
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
        util = new ItemUtilExt();
        Item item = this.util.GetItem(data.Value);
        if (item == null)
        {
          messageBarResponse.Type = "error";
          messageBarResponse.Message = "Message could not be found.";
          return messageBarResponse;
        }

        TypeResolverFactory tFactory = new TypeResolverFactory();
        TypeResolver tResolver = tFactory.GetTypeResolver();

        MessageItem messageItem = tResolver.GetCorrectMessageObject(item);

        ScheduleItem scheduleItemByMessageType = util.GetScheduleItemByMessageType(messageItem);
        if (scheduleItemByMessageType != null)
        {
          scheduleItemByMessageType.Remove();
        }

        messageBarResponse.Type = "notification";
        messageBarResponse.Message = "The message " + item.DisplayName + " has been deleted.";
        item.Delete();
        this.DeleteCampaign(item.ID.Guid);
      }
      catch (Exception ex)
      {
        this._logger.LogError(ex.Message, ex);
        messageBarResponse.Error = true;
      }
      return messageBarResponse;
    }

    private void DeleteCampaign(Guid messageID)
    {
      if (!String.IsNullOrEmpty(this.ConnectionString))
      {
        using (SqlConnection sqlConnection = new SqlConnection(this.ConnectionString))
        {
          sqlConnection.Open();
          using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
          {
            sqlCommand.CommandText = "DELETE FROM Campaigns WHERE MessageID=@MessageID";
            sqlCommand.Parameters.Add(new SqlParameter("@MessageID", messageID));
            sqlCommand.ExecuteNonQuery();
          }
        }
      }
    }

  }
}
