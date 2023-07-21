using R2API;

namespace HIFUCaptainTweaks
{
    public abstract class MiscBase
    {
        public abstract string Name { get; }
        public virtual bool isEnabled { get; } = true;

        public T ConfigOption<T>(T value, string name, string description)
        {
            var config = Main.HCAPTConfig.Bind<T>(Name, name, value, description);
            ConfigManager.HandleConfig<T>(config, Main.HCAPTBackupConfig, name);
            return config.Value;
        }

        public abstract void Hooks();

        public string d(float f)
        {
            return (f * 100f).ToString() + "%";
        }

        public virtual void Init()
        {
            Hooks();
            Main.HCAPTLogger.LogInfo("Added " + Name);
        }
    }
}