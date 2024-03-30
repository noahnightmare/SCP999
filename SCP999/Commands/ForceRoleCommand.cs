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
using PluginAPI.Events;

namespace SCP999.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class ForceRoleCommand : ICommand
    {
        public string Command => "force999";
        public string[] Aliases => ["f999"];
        public string Description => "Forcer un joueur à incarner SCP 999";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("999.set"))
            {
                response = "Vous n'êtes pas autorisé à utiliser cette commande.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "Vous n'avez pas spécifié de joueur à définir. Utilisez force999 <nom du joueur>";
                return false;
            }

            string args = arguments.At(0);

            if (args == "*" || args == "all")
            {
                foreach (Player player in Player.List.Where(x => !CustomRole.Get(typeof(CustomRoleScp999)).Check(x)))
                {
                    CustomRole.Get(typeof(CustomRoleScp999)).AddRole(player);
                }
            }
            else
            {
                if (!Player.List.Contains(Player.Get(args)))
                {
                    response = "Joueur non trouvé. Utilisez * ou all pour définir tout le monde sur 999.";
                    return false;
                }

                if (CustomRole.Get(typeof(CustomRoleScp999)).Check(Player.Get(args)))
                {
                    response = "Le joueur est déjà SCP 999 !";
                    return false;
                }

                CustomRole.Get(typeof(CustomRoleScp999)).AddRole(Player.Get(args));
            }

            response = "Joueur(s) défini(s) sur 999 !";
            return true;
        }
    }
}
