using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace NMPB_GUI.ReflectionControls
{
	public class ReflectionManager<T> : IDisposable
	{
		private object _obj;

		private readonly Dictionary<string, FieldInfo> _fields;

		private readonly Dictionary<string, PropertyInfo> _properties;

		public Dictionary<string, IControlWrapper> Controls;

		private Timer _timer;

		public ReflectionManager(T obj, bool generateWrappers = true)
		{
			this._obj = obj;
			this._fields = ((IEnumerable<FieldInfo>)typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public)).ToDictionary<FieldInfo, string>((FieldInfo info) => info.Name);
			this._properties = ((IEnumerable<PropertyInfo>)typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)).ToDictionary<PropertyInfo, string>((PropertyInfo info) => info.Name);
			if (generateWrappers)
			{
				foreach (KeyValuePair<string, FieldInfo> _field in this._fields)
				{
					this.CreateWrapper(_field.Key, true, _field.Value.FieldType, _field.Value.GetValue(this._obj));
				}
				foreach (KeyValuePair<string, PropertyInfo> _property in this._properties)
				{
					this.CreateWrapper(_property.Key, _property.Value.GetSetMethod() != null, _property.Value.PropertyType, _property.Value.GetValue(this._obj, null));
				}
			}
			this._timer = new Timer()
			{
				Interval = 100,
				Enabled = true
			};
			this._timer.Tick += new EventHandler(this.UpdateValues);
		}

		public static string AddSpacesToSentence(string text, bool preserveAcronyms)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder(text.Length * 2);
			stringBuilder.Append(text[0]);
			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]) && (text[i - 1] != ' ' && !char.IsUpper(text[i - 1]) || preserveAcronyms && char.IsUpper(text[i - 1]) && i < text.Length - 1 && !char.IsUpper(text[i + 1])))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(text[i]);
			}
			return stringBuilder.ToString();
		}

		private void CreateWrapper(string name, bool enabled, Type type, object value)
		{
			IControlWrapper checkBoxWrapper = null;
			if (type == typeof(bool))
			{
				checkBoxWrapper = new CheckBoxWrapper(name, ReflectionManager<T>.AddSpacesToSentence(name, true), enabled, (bool)value);
			}
			if (type == typeof(int))
			{
				checkBoxWrapper = new NumericUpDownWrapper(name, ReflectionManager<T>.AddSpacesToSentence(name, true), enabled, (int)value);
			}
			this.RegisterWrapper(name, checkBoxWrapper);
		}

		public void Dispose()
		{
			this._timer.Dispose();
			foreach (KeyValuePair<string, IControlWrapper> control in this.Controls)
			{
				control.Value.Dispose();
			}
		}

		private void OnChanged(object sender, EventArgs e)
		{
			IControlWrapper controlWrapper = sender as IControlWrapper;
			if (controlWrapper == null)
			{
				return;
			}
			if (this._fields.ContainsKey(controlWrapper.Name))
			{
				this._fields[controlWrapper.Name].SetValue(this._obj, controlWrapper.Value);
			}
			if (this._properties.ContainsKey(controlWrapper.Name))
			{
				this._properties[controlWrapper.Name].SetValue(this._obj, controlWrapper.Value, null);
			}
		}

		public void RegisterWrapper(string name, IControlWrapper wrapper)
		{
			if (wrapper == null)
			{
				return;
			}
			wrapper.Changed += new EventHandler(this.OnChanged);
			this.Controls.Add(name, wrapper);
		}

		private void UpdateValues(object sender, EventArgs e)
		{
			foreach (KeyValuePair<string, IControlWrapper> control in this.Controls)
			{
				string name = control.Value.Name;
				if (this._fields.ContainsKey(name))
				{
					control.Value.Value = this._fields[name].GetValue(this._obj);
				}
				if (!this._properties.ContainsKey(name))
				{
					continue;
				}
				control.Value.Value = this._properties[name].GetValue(this._obj, null);
			}
		}
	}
}