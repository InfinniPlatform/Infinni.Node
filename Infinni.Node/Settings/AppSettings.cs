using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;

namespace Infinni.Node.Settings
{
	/// <summary>
	/// Предоставляет доступ к настройкам приложения.
	/// </summary>
	internal static class AppSettings
	{
		/// <summary>
		/// Возвращает значение настройки.
		/// </summary>
		/// <param name="name">Наименование настройки.</param>
		/// <param name="defaultValue">Значение настройки по умолчанию.</param>
		public static string GetValue(string name, string defaultValue = null)
		{
			var value = ConfigurationManager.AppSettings[name];

			return value ?? defaultValue;
		}

		/// <summary>
		/// Возвращает значение настройки.
		/// </summary>
		/// <typeparam name="T">Тип настройки.</typeparam>
		/// <param name="name">Наименование настройки.</param>
		/// <param name="defaultValue">Значение настройки по умолчанию.</param>
		public static T GetValue<T>(string name, T defaultValue = default(T))
		{
			var result = defaultValue;

			var value = ConfigurationManager.AppSettings[name];

			if (string.IsNullOrEmpty(value) == false)
			{
				var converter = TypeDescriptor.GetConverter(typeof(T));
				result = (T)converter.ConvertFromInvariantString(value);
			}

			return result;
		}

		/// <summary>
		/// Возвращает значение настройки.
		/// </summary>
		/// <typeparam name="T">Тип настройки.</typeparam>
		/// <param name="name">Наименование настройки.</param>
		/// <param name="defaultValue">Значение настройки по умолчанию.</param>
		public static T[] GetValues<T>(string name, params T[] defaultValue)
		{
			var result = defaultValue;

			var value = ConfigurationManager.AppSettings[name];

			if (string.IsNullOrEmpty(value) == false)
			{
				var items = value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

				if (items.Length > 0)
				{
					var converter = TypeDescriptor.GetConverter(typeof(T));
					result = items.Select(i => (T)converter.ConvertFromInvariantString(i)).ToArray();
				}
			}

			return result;
		}
	}
}