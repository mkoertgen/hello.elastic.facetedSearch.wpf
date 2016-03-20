using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Caliburn.Micro;
using TriggerBase = System.Windows.Interactivity.TriggerBase;

// ReSharper disable once CheckNamespace
namespace HelloFacets.MiniMods
{

    public static class ShortcutParser
    {
        private static bool _isAttached;

        public static void AttachToCaliburn()
        {
            if (_isAttached) return;

            var currentParser = Parser.CreateTrigger;
            Parser.CreateTrigger = (target, triggerText) => CanParse(triggerText)
                ? CreateTrigger(triggerText)
                : currentParser(target, triggerText);
            _isAttached = true;
        }

        public static bool CanParse(string triggerText)
        {
            return !string.IsNullOrWhiteSpace(triggerText) && triggerText.Contains("Shortcut");
        }

        public static TriggerBase CreateTrigger(string triggerText)
        {
            if (triggerText == null) throw new ArgumentNullException(nameof(triggerText));

            var triggerDetail = triggerText
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Replace("Shortcut", string.Empty)
                .Trim();

            var modKeys = ModifierKeys.None;

            var allKeys = triggerDetail.Split('+');
            var key = (Key)Enum.Parse(typeof(Key), allKeys.Last());

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var modifierKey in allKeys.Take(allKeys.Count() - 1))
            {
                modKeys |= (ModifierKeys)Enum.Parse(typeof(ModifierKeys), modifierKey);
            }

            var keyBinding = new KeyBinding(new InputBindingTrigger(), key, modKeys);
            var trigger = new InputBindingTrigger { InputBinding = keyBinding };
            return trigger;
        }
    }

    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty = DependencyProperty.Register("InputBinding", 
            typeof(InputBinding), typeof(InputBindingTrigger), new UIPropertyMetadata(null));

        public InputBinding InputBinding
        {
            get { return (InputBinding)GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }

        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter)
        {
            // action is anyway blocked by Caliburn at the invoke level
            return true;
        }

        public void Execute(object parameter)
        {
            InvokeActions(parameter);
        }

        protected override void OnAttached()
        {
            if (InputBinding != null)
            {
                InputBinding.Command = this;
                if (AssociatedObject.Focusable)
                    AssociatedObject.InputBindings.Add(InputBinding);
                else
                {
                    Window window = null;
                    AssociatedObject.Loaded += (s, e) =>
                        {
                            window = GetWindow(AssociatedObject);
                            if (!window.InputBindings.Contains(InputBinding))
                                window.InputBindings.Add(InputBinding);
                        };
                    AssociatedObject.Unloaded += (s,e) => window.InputBindings.Remove(InputBinding);
                }
            }

            base.OnAttached();
        }

        private static Window GetWindow(FrameworkElement frameworkElement)
        {
            var window = frameworkElement as Window;
            if (window != null)
                return window;

            var parent = (FrameworkElement)frameworkElement.Parent;
            return GetWindow(parent);
        }
    }
}