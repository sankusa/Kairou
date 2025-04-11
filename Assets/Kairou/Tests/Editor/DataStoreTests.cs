using System;
using Kairou;
using NUnit.Framework;

namespace Kairou.Tests
{
    public class DataStoreTests
    {
        [Test]
        public void SetValue_ValidKeyAndValue_ValueSetCorrectly()
        {
            var dataStore = new DataStore();
            string key = "testKey";
            int value = 42;

            dataStore.SetValue(key, value);
            Assert.AreEqual(value, dataStore.GetValue(key, 0));
        }

        [Test]
        public void GetValue_KeyNotFound_ReturnsDefaultValue()
        {
            var dataStore = new DataStore();
            string key = "nonExistentKey";
            int defaultValue = -1;

            int result = dataStore.GetValue(key, defaultValue);
            Assert.AreEqual(defaultValue, result);
        }

        [Test]
        public void HasKey_KeyExists_ReturnsTrue()
        {
            var dataStore = new DataStore();
            string key = "existingKey";
            dataStore.SetValue(key, 100);

            Assert.IsTrue(dataStore.HasKey(key));
        }

        [Test]
        public void HasKey_KeyDoesNotExist_ReturnsFalse()
        {
            var dataStore = new DataStore();
            string key = "nonExistentKey";

            Assert.IsFalse(dataStore.HasKey(key));
        }

        [Test]
        public void TryGetValue_KeyExists_ReturnsTrueAndValue()
        {
            var dataStore = new DataStore();
            string key = "existingKey";
            int expectedValue = 123;
            dataStore.SetValue(key, expectedValue);

            bool result = dataStore.TryGetValue(key, out int actualValue);
            Assert.IsTrue(result);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void TryGetValue_KeyDoesNotExist_ReturnsFalseAndDefault()
        {
            var dataStore = new DataStore();
            string key = "nonExistentKey";

            bool result = dataStore.TryGetValue(key, out int actualValue);
            Assert.IsFalse(result);
            Assert.AreEqual(0, actualValue); // Default value for int
        }

        [Test]
        public void RemoveKey_KeyExists_RemovesKey()
        {
            var dataStore = new DataStore();
            string key = "keyToRemove";
            dataStore.SetValue(key, 200);

            dataStore.RemoveKey(key);
            Assert.IsFalse(dataStore.HasKey(key));
        }

        [Test]
        public void Clear_KeysExist_ClearsAllKeys()
        {
            var dataStore = new DataStore();
            dataStore.SetValue("key1", 1);
            dataStore.SetValue("key2", 2);

            dataStore.Clear();

            Assert.IsFalse(dataStore.HasKey("key1"));
            Assert.IsFalse(dataStore.HasKey("key2"));
        }

        [Test]
        public void ToText_And_FromText_ValuesExist()
        {
            var dataStore = new DataStore();
            dataStore.SetValue("key1", 1);
            dataStore.SetValue("key2", "stringValue");

            string expectedStart = "{\"_version\":1";
            string text = dataStore.ToText();
            Assert.IsTrue(text.StartsWith(expectedStart));

            var newDataStore = new DataStore();
            newDataStore.FromText(text);
            Assert.IsTrue(newDataStore.GetValue<int>("key1") == 1);
            Assert.IsTrue(newDataStore.GetValue<string>("key2") == "stringValue");
        }
    }
}