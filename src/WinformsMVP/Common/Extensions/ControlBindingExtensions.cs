using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace WinformsMVP.Common.Extensions
{
    public static class ControlBindingExtensions
    {
        public static void BindProperty<TControl, TViewModel, TValue>(
            this TControl control,
            TViewModel viewModel,
            Expression<Func<TViewModel, TValue>> propertyExpression,
            string controlPropertyName)
            where TControl : Control
            where TViewModel : INotifyPropertyChanged
        {
            if (control == null || viewModel == null || propertyExpression == null) return;

            var propertyName = PropertyNameHelper.GetName(propertyExpression);

            control.DataBindings.Clear();
            control.DataBindings.Add(controlPropertyName, viewModel, propertyName, false, DataSourceUpdateMode.OnPropertyChanged);
        }

        public static void Bind<TViewModel>(this TextBox textBox, TViewModel viewModel, Expression<Func<TViewModel, object>> propertyExpression)
            where TViewModel : INotifyPropertyChanged
        {
            textBox.BindProperty(viewModel, propertyExpression, nameof(textBox.Text));
        }

        public static void Bind<TViewModel>(this CheckBox checkBox, TViewModel viewModel, Expression<Func<TViewModel, bool>> propertyExpression)
            where TViewModel : INotifyPropertyChanged
        {
            checkBox.BindProperty(viewModel, propertyExpression, nameof(checkBox.Checked));
        }

        public static void Bind<TViewModel>(this NumericUpDown numericUpDown, TViewModel viewModel, Expression<Func<TViewModel, decimal>> propertyExpression)
            where TViewModel : INotifyPropertyChanged
        {
            numericUpDown.BindProperty(viewModel, propertyExpression, nameof(numericUpDown.Value));
        }

        public static void Bind<TViewModel, TValue>(this ComboBox comboBox, TViewModel viewModel, Expression<Func<TViewModel, TValue>> propertyExpression)
            where TViewModel : INotifyPropertyChanged
        {
            comboBox.BindProperty(viewModel, propertyExpression, nameof(comboBox.SelectedValue));
        }

        public static void BindRadioGroup<TViewModel, TValue>(this IEnumerable<KeyValuePair<RadioButton, TValue>> radioPairs, TViewModel viewModel, Expression<Func<TViewModel, TValue>> propertyExpression) where TViewModel : INotifyPropertyChanged
        {
            if (radioPairs == null || viewModel == null || propertyExpression == null)
                return;

            var propertyName = PropertyNameHelper.GetName(propertyExpression);
            var propInfo = viewModel.GetType().GetProperty(propertyName);
            if (propInfo == null) throw new ArgumentException($"Property {propertyName} not found.");

            foreach (var kv in radioPairs)
            {
                var radio = kv.Key;
                var value = kv.Value;

                radio.Checked = Equals(propInfo.GetValue(viewModel), value);

                radio.CheckedChanged -= RadioButton_CheckedChanged;
                radio.CheckedChanged += RadioButton_CheckedChanged;

                void RadioButton_CheckedChanged(object sender, EventArgs e)
                {
                    if (radio.Checked)
                    {
                        var current = propInfo.GetValue(viewModel);
                        if (!Equals(current, value))
                            propInfo.SetValue(viewModel, value);
                    }
                }
            }

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    var currentValue = propInfo.GetValue(viewModel);
                    foreach (var kv in radioPairs)
                    {
                        kv.Key.Checked = Equals(currentValue, kv.Value);
                    }
                }
            };
        }
    }
}
