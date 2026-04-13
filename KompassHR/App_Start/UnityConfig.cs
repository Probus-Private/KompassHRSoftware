using System.Web.Mvc;
using Unity;
using Unity.Mvc5;
using KompassHR;

namespace KompassHR
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

           // container.RegisterType<ILeave_Entry_Master_Service, Leave_Entry_Master_Service>();


            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}