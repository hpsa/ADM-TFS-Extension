using System;
using System.Collections.Generic;
using System.Linq;

namespace PSModule
{
	public static class Extensions
	{
		public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue = default)
		{
			if (dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			return defaultValue;
		}

		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		public static bool IsEmptyOrWhiteSpace(this string str)
		{
			if (str != null)
			{
				return str.Trim() == string.Empty;
			}
			return false;
		}

		public static bool IsValidUrl(this string url)
		{
			return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
		}

		public static bool EqualsIgnoreCase(this string s1, string s2)
		{
			return s1?.Equals(s2, StringComparison.OrdinalIgnoreCase) ?? (s2 == null);
		}

		public static bool In(this string str, bool ignoreCase, params string[] values)
		{
			if (ignoreCase)
			{
				return values?.Any((string s) => EqualsIgnoreCase(str, s)) ?? (str == null);
			}
			return In(str, values);
		}

		public static bool In<T>(this T obj, params T[] values)
		{
			return values?.Any((T o) => Equals(obj, o)) ?? false;
		}

		public static bool IsNullOrEmpty<T>(this T[] arr)
		{
			if (arr != null)
			{
				return arr.Length == 0;
			}
			return true;
		}

		public static bool IsNullOrEmpty<T>(this IList<T> arr)
		{
			if (arr != null)
			{
				return arr.Count == 0;
			}
			return true;
		}

		public static bool IsNull(this DateTime dt)
		{
			if (Convert.GetTypeCode(dt) != 0 && (dt.Date != DateTime.MinValue.Date))
			{
				return dt.Date == DateTime.MaxValue.Date;
			}
			return true;
		}

		public static bool IsNullOrEmpty(this DateTime? dt)
		{
			if (dt.HasValue)
			{
				return IsNull(dt.Value);
			}
			return true;
		}

		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (T item in enumeration)
			{
				action(item);
			}
		}
	}
}