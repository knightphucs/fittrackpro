// AsyncStorage utility functions
import AsyncStorage from "@react-native-async-storage/async-storage";

/**
 * Get item from AsyncStorage with type safety
 */
export const getStorageItem = async <T>(key: string): Promise<T | null> => {
  try {
    const value = await AsyncStorage.getItem(key);
    return value ? JSON.parse(value) : null;
  } catch (error) {
    console.error(`Error getting storage item ${key}:`, error);
    return null;
  }
};

/**
 * Set item in AsyncStorage
 */
export const setStorageItem = async <T>(
  key: string,
  value: T
): Promise<void> => {
  try {
    await AsyncStorage.setItem(key, JSON.stringify(value));
  } catch (error) {
    console.error(`Error setting storage item ${key}:`, error);
  }
};

/**
 * Remove item from AsyncStorage
 */
export const removeStorageItem = async (key: string): Promise<void> => {
  try {
    await AsyncStorage.removeItem(key);
  } catch (error) {
    console.error(`Error removing storage item ${key}:`, error);
  }
};

/**
 * Remove multiple items from AsyncStorage
 */
export const removeStorageItems = async (keys: string[]): Promise<void> => {
  try {
    await AsyncStorage.multiRemove(keys);
  } catch (error) {
    console.error("Error removing storage items:", error);
  }
};

/**
 * Clear all AsyncStorage data
 */
export const clearStorage = async (): Promise<void> => {
  try {
    await AsyncStorage.clear();
  } catch (error) {
    console.error("Error clearing storage:", error);
  }
};

/**
 * Get all keys from AsyncStorage
 */
export const getAllStorageKeys = async (): Promise<string[]> => {
  try {
    return (await AsyncStorage.getAllKeys()) as string[];
  } catch (error) {
    console.error("Error getting all storage keys:", error);
    return [];
  }
};
