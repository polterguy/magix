/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magix.Core
{
    /**
     * Level4: Helps load Active Modules embedded in resources. Relies on that Magix.Brix.Loader.AssemblyResourceProvider
     * is registered as a Virtual Path Provider in e.g. your Global.asax file. Use the Instance method
     * to access the singleton object, then use the LoadControl to load UserControls embedded as resources.
     * Kind of like the Magix' version of Page.LoadControl. Can be used directly by you, if you really
     * know what you're doing though. In general, I'd say DON'T ...!!
     */
    public sealed class ModuleControllerLoader
    {
        private static ModuleControllerLoader _instance;

		private readonly Dictionary<string, Tuple<string, Type>> _moduleTypes = 
			new Dictionary<string, Tuple<string, Type>>();

		private readonly List<Type> _controllerTypes = new List<Type>();

		private static List<Assembly> _assemblies;

        private delegate void TypeDelegate(Type type);

		private static Dictionary<Type, List<Tuple<MethodInfo, string>>> _cacheMethodsForType = 
			new Dictionary<Type, List<Tuple<MethodInfo, string>>>();

		private static Dictionary<Type, List<Tuple<MethodInfo, string>>> _cacheStaticMethodsForType = 
			new Dictionary<Type, List<Tuple<MethodInfo, string>>>();


        private ModuleControllerLoader()
        {
            // Making sure all DLLs are loaded
            MakeSureAllDLLsAreLoaded();

            // Initializing all Active Modules
            FindAllTypesWithAttribute<ActiveModuleAttribute>(
                delegate(Type type)
                {
                    string userControlFile = type.FullName + ".ascx";
                    _moduleTypes[type.FullName] = new Tuple<string, Type>(userControlFile, type);

					// Initializing Static Event handlers ONCE and ONCE ONLY
                    InitializeEventHandlers(null, type);
                });

            // Initializing all Active Controllers
            FindAllTypesWithAttribute<ActiveControllerAttribute>(
                delegate(Type type)
                {
                    _controllerTypes.Add(type);

					// Initializing Static Event handlers ONCE and ONCE ONLY
                    InitializeEventHandlers(null, type);
                });
        }

        /**
         * Level4: A list of all your ActiveControllers in the system
         */
        public IEnumerable<Type> ActiveControllerTypes
        {
            get
            {
                foreach (Type idx in _controllerTypes)
				{
					yield return idx;
				}
            }
        }

        /**
         * Level4: A list of all your ActiveModules with its associated containing Assembly 
         * File from within the system
         */
        public IEnumerable<Tuple<string, Type>> ActiveModulesTypes
        {
            get
            {
                foreach (Tuple<string, Type> idx in _moduleTypes.Values)
				{
					yield return idx;
				}
            }
        }

        /**
         * Level4: A list of all your ActiveModules Loading Name Keys
         */
        public IEnumerable<string> ActiveModulesKeys
        {
            get
            {
                foreach (string idx in _moduleTypes.Keys)
				{
					yield return idx;
				}
            }
        }

		/**
		 * Level4: Returns the given ActiveModule Type according to key, with its associated Assembly Name
		 */
		public Tuple<string, Type> ActiveModuleValue(string key)
		{
			return _moduleTypes[key];
		}

        // Helper method that loops through every type in AppDomain and 
        // looks for an attribute of a given type and passes it into a delegate 
        // submitted by the caller...
        private static void FindAllTypesWithAttribute<TAttrType>(TypeDelegate functor)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Consciously skipping Aall GAC assemblies since these are 
                // expected to be .Net Framework assemblies...
                if (assembly.GlobalAssemblyCache)
                    continue;
                foreach (Type type in assembly.GetTypes())
                {
                    TAttrType[] attributes = type.GetCustomAttributes(
                        typeof(TAttrType),
                        true) as TAttrType[];
                    if (attributes != null && attributes.Length > 0)
                    {
                        // Calling our given delegate with the type...
                        functor(type);
                    }
                }
            }
        }

        /**
         * Level4: Singleton accessor. Allows access to the 'one and only' PluginLoader
         */
        public static ModuleControllerLoader Instance
        {
            [DebuggerStepThrough]
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ModuleControllerLoader))
                    {
                        if (_instance == null)
                        {
                            _instance = new ModuleControllerLoader();

                            // Fire the "Application Startup" event. This one will only trigger
                            // ONCE in comparison to the "Magix.Core.InitialLoading" event which will fire
                            // every time the page reloads...
							Node tmp = new Node();
							tmp["initial-startup-of-process"].Value = null;
                            ActiveEvents.Instance.RaiseActiveEvent(
                                null, 
                                "magix.core.application-startup",
								tmp);
                        }
                    }
                }
                return _instance;
            }
        }

        /**
         * Level3: Dynamically load a Control with the given FullName (namespace + type name). This
         * is the method which is internally used in Magix-Brix to load UserControls from 
         * embedded resources and also other controls. Since ActiveEvents might be mapped and
         * overridden, you actually have no guarantee of that the event you wish to raise
         * is the one who will become raised
         */
        public Control LoadActiveModule(string fullTypeName)
        {
			Page page = (HttpContext.Current.Handler as Page);

            // Checking to see if we've got our UnLoad event handlers event here...
            if (page.Items["__Ra.Brix.Loader.PluginLoader.hasInstantiatedControllers"] == null)
            {
                page.Items["__Ra.Brix.Loader.PluginLoader.hasInstantiatedControllers"] = true;
                InstantiateAllControllers();
            }

            if (!_moduleTypes.ContainsKey(fullTypeName))
            {
                throw new ArgumentException(
                    "Couldn't find the plugin with the name of; '" + fullTypeName + "'");
            }
            Tuple<string, Type> pluginType = _moduleTypes[fullTypeName];

			Control retVal =
                page.LoadControl(
                    "~/Magix.Brix.Module/" +
                    pluginType.Item2.Assembly.ManifestModule.ScopeName +
                    "/" +
                    pluginType.Item1);
            InitializeEventHandlers(retVal, pluginType.Item2);
            return retVal;
        }

        private void InstantiateAllControllers()
        {
            foreach (Type idxType in _controllerTypes)
            {
				if (idxType.GetConstructor(System.Type.EmptyTypes) != null)
				{
	                object controllerObject = idxType.GetConstructor(System.Type.EmptyTypes).Invoke(null);
	                InitializeEventHandlers(controllerObject, idxType);
				}
            }
        }

		private static void BuildCacheForStaticMethodsForType (Type pluginType)
		{
			_cacheStaticMethodsForType [pluginType] = new List<Tuple<MethodInfo, string>> ();
			foreach (MethodInfo idx in pluginType.GetMethods (
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
			{
				ActiveEventAttribute[] attr = 
					idx.GetCustomAttributes (typeof(ActiveEventAttribute), true) as ActiveEventAttribute[];
				if (attr == null || attr.Length <= 0)
					continue;
				foreach (ActiveEventAttribute idxAttr in attr)
				{
					_cacheStaticMethodsForType [pluginType].Add (
						new Tuple<MethodInfo, string> (idx, idxAttr.Name));
				}
			}
		}

		private static void BuildCacheForMethodsForType (Type pluginType)
		{
			_cacheMethodsForType [pluginType] = new List<Tuple<MethodInfo, string>> ();
			foreach (MethodInfo idx in pluginType.GetMethods (
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				ActiveEventAttribute[] attr = 
					idx.GetCustomAttributes (typeof(ActiveEventAttribute), true) as ActiveEventAttribute[];
				if (attr == null || attr.Length <= 0)
					continue;
				foreach (ActiveEventAttribute idxAttr in attr)
				{
					_cacheMethodsForType [pluginType].Add (
						new Tuple<MethodInfo, string> (idx, idxAttr.Name));
				}
			}
		}

        private static void InitializeEventHandlers (object objectInstance, Type pluginType)
		{
			// If the context passed is null, then what we're trying to retrieve
			// are the stat event handlers...
			if (objectInstance == null)
			{
				if (!_cacheStaticMethodsForType.ContainsKey(pluginType))
				{
					BuildCacheForStaticMethodsForType (pluginType);
				}
				foreach (Tuple<MethodInfo, string> idx in _cacheStaticMethodsForType[pluginType])
				{
                    ActiveEvents.Instance.AddListener(null, idx.Item1, idx.Item2);
				}
			}
			else
			{
				if (!_cacheMethodsForType.ContainsKey(pluginType))
				{
					BuildCacheForMethodsForType (pluginType);
				}
				foreach (Tuple<MethodInfo, string> idx in _cacheMethodsForType[pluginType])
				{
                    ActiveEvents.Instance.AddListener(objectInstance, idx.Item1, idx.Item2);
				}
			}
        }

        private static void MakeSureAllDLLsAreLoaded()
        {
            // Sometimes not all DLLs in the bin folder will be included in the
            // current AppDomain. This often happens due to that no types from
            // the DLLs are actually references within the website itself
            // This logic runs through all DLLs in the bin folder of the
            // website to check if they're inside the current AppDomain, and
            // if not loads them up
            List<Assembly> initialAssemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            DirectoryInfo di = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/bin"));
            LoadDLLsFromDirectory(di, initialAssemblies);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static void LoadDLLsFromDirectory(DirectoryInfo di, List<Assembly> initialAssemblies)
        {
            foreach (FileInfo idxFile in di.GetFiles("*.dll"))
            {
                try
                {
                    FileInfo info = idxFile;
                    if (initialAssemblies.Exists(
                        delegate(Assembly idx)
                            {
                                return idx.ManifestModule.Name == info.Name;
                            }))
                        continue;
                    Assembly.LoadFrom(idxFile.FullName);
                }
                catch (Exception)
                {
                    ; // Intentionally do nothing in case assembly loading throws...!
                    // Especially true for C++ compiled assemblies...
                    // Sample here is the MySQL DLL...
                }
            }

			// Recursively traversing sub directories
            foreach (DirectoryInfo idxChild in di.GetDirectories())
            {
                LoadDLLsFromDirectory(idxChild, initialAssemblies);
            }
        }

        /*
         * Internally used in AssemblyResolve
         */
        internal static List<Assembly> ModuleAssemblies
        {
            get
            {
                if (_assemblies != null)
                    return _assemblies;

                _assemblies = new List<Assembly>();

                foreach (Assembly idx in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (idx.GlobalAssemblyCache)
                        continue;
                    try
                    {
                        _assemblies.Add(idx);
                    }
                    catch (Exception)
                    {
                        ; // Intentionally do nothing...!
                    }
                }
                return _assemblies;
            }
        }

        static private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            string name = e.Name;
            if (name.Contains(","))
            {
                name = name.Substring(0, name.IndexOf(",")).ToLower();
            }
            foreach (Assembly idx in ModuleAssemblies)
            {
                if (idx.CodeBase.Substring(idx.CodeBase.LastIndexOf("/") + 1).ToLower().Replace(".dll", "") == name)
                    return idx;
            }
            return null;
        }
    }
}
