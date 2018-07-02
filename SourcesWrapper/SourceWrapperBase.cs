using Sync.Plugins;
using System;

namespace DifficultParamModifyPlugin.SourcesWrapper
{
    public abstract class SourceWrapperBase<T> : SourceWrapperBase where T : Plugin
    {
        private T ref_plugin;

        private DifficultParamModifyPlugin ref_panel;

        public T RefPlugin { get => ref_plugin; }

        public DifficultParamModifyPlugin RefPanelPlugin { get => ref_panel; }

        public SourceWrapperBase(T ref_plugin, DifficultParamModifyPlugin plugin)
        {
            this.ref_plugin = ref_plugin;
            this.ref_panel = plugin;
        }
    }

    public abstract class SourceWrapperBase
    {
        /// <summary>
        /// Listen=false
        /// Play=true
        /// </summary>
        public bool CurrentOutputType { get; protected set; }

        public abstract void Detach();

        public abstract bool Attach();
    }
}