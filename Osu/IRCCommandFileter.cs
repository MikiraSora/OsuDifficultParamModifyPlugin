using Sync.MessageFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DifficultParamModifyPlugin.Osu
{
    class IRCCommandFileter : IFilter, ISourceClient
    {
        const string COMMAND = "?modify_diff";

        public IRCCommandFileter()
        {
        }

        public void onMsg(ref IMessageBase msg)
        {
            if (msg.Message.RawText.StartsWith(COMMAND))
            {
                msg.Cancel = true;

                var command = msg.Message.RawText.Remove(0,1);

                Sync.SyncHost.Instance.Commands.invokeCmdString(command);

                Sync.SyncHost.Instance.ClientWrapper.Client.SendMessage(new IRCMessage(msg.User.RawText, "Exec!"));
            }
        }
    }
}
