﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.BackEnd;
using Microsoft.Build.Framework;

#nullable disable

namespace Microsoft.Build.UnitTests.BackEnd
{
    /// <summary>
    /// Class containing methods used to assist in testing serialization methods.
    /// </summary>
    internal static class TranslationHelpers
    {
        /// <summary>
        /// The stream backing the serialization classes.
        /// </summary>
        private static MemoryStream s_serializationStream;

        /// <summary>
        /// Gets a serializer used to write data.  Note that only one such serializer may be used from this class at a time.
        /// </summary>
        internal static ITranslator GetWriteTranslator()
        {
            s_serializationStream = new MemoryStream();
            return BinaryTranslator.GetWriteTranslator(s_serializationStream);
        }

        /// <summary>
        /// Gets a serializer used to read data.  Note that only one such serializer may be used from this class at a time,
        /// and this must be called after GetWriteTranslator() has been called.
        /// </summary>
        internal static ITranslator GetReadTranslator()
        {
            s_serializationStream.Seek(0, SeekOrigin.Begin);
            return BinaryTranslator.GetReadTranslator(s_serializationStream, null);
        }

        /// <summary>
        /// Compares two collections.
        /// </summary>
        /// <typeparam name="T">The collections element type.</typeparam>
        /// <param name="left">The left collections.</param>
        /// <param name="right">The right collections.</param>
        /// <param name="comparer">The comparer to use on each element.</param>
        /// <returns>True if the collections are equivalent.</returns>
        internal static bool CompareCollections<T>(ICollection<T> left, ICollection<T> right, IComparer<T> comparer)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((left == null) ^ (right == null))
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            T[] leftArray = left.ToArray();
            T[] rightArray = right.ToArray();

            for (int i = 0; i < leftArray.Length; i++)
            {
                if (comparer.Compare(leftArray[i], rightArray[i]) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two exceptions.
        /// </summary>
        internal static bool CompareExceptions(Exception left, Exception right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if ((left == null) ^ (right == null))
            {
                return false;
            }

            if (left.Message != right.Message)
            {
                return false;
            }

            if (left.StackTrace != right.StackTrace)
            {
                return false;
            }

            return CompareExceptions(left.InnerException, right.InnerException);
        }

        internal static string GetPropertiesString(IEnumerable properties)
        {
            var dictionary = properties
                .OfType<DictionaryEntry>()
                .ToDictionary(
                    (Func<DictionaryEntry, string>)(d => d.Key.ToString()),
                    (Func<DictionaryEntry, string>)(d => d.Value.ToString()));
            return ToString(dictionary);
        }

        internal static string GetMultiItemsString(IEnumerable items)
        {
            var list = items
                .OfType<DictionaryEntry>()
                .Select(i => i.Key.ToString() + GetTaskItemString(i.Value));
            var text = string.Join("\n", list);
            return text;
        }

        internal static string GetItemsString(IEnumerable items)
        {
            var list = items
                .OfType<object>()
                .Select(i => GetTaskItemString(i));
            var text = string.Join("\n", list);
            return text;
        }

        internal static string GetTaskItemString(object item)
        {
            var sb = new StringBuilder();

            if (item is ITaskItem taskItem)
            {
                sb.Append(taskItem.ItemSpec);
                foreach (string name in taskItem.MetadataNames)
                {
                    var value = taskItem.GetMetadata(name);
                    sb.Append($";{name}={value}");
                }
            }
            else
            {
                sb.Append(Convert.ToString(item));
            }

            return sb.ToString();
        }

        internal static string ToString(IDictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return "null";
            }

            return string.Join(";", dictionary.Select(kvp => kvp.Key + "=" + kvp.Value));
        }
    }
}
