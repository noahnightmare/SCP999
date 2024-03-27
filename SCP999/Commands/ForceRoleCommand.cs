using CommandSystem;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.CustomRoles.API.Features;
using SCP999.Role;

namespace SCP999.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ForceRoleCommand : ICommand
    {
        public string Command => "force999";
        public string[] Aliases => ["f999"];
        public string Description => "Force a player to play as SCP 999 - Mainly for testing";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("999.set"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "You didn't specify a player to set. Use force999 <player name>";
                return false;
            }

            if (!Player.List.Contains(Player.Get(arguments.At(0))))
            {
                response = "Player not found. Use their nickname!";
                return false;
            }

            CustomRole.Get(typeof(CustomRoleScp999)).AddRole(Player.Get(arguments.At(0)));

            response = "Player set as 999!";
            return true;
        }
    }
}
