// storageUtils.js

// Store data with an expiration time in local storage
function setLocalStorageWithExpiration(key, value, ttl) {
    const now = new Date();
    const item = {
        value: value,
        expiry: now.getTime() + ttl,
    };
    localStorage.setItem(key, JSON.stringify(item));
}

// Retrieve data with expiration check from local storage
function getLocalStorageWithExpiration(key) {
    const itemStr = localStorage.getItem(key);
    if (!itemStr) {
        return null;
    }

    const item = JSON.parse(itemStr);
    const now = new Date();

    // Check if the item is expired
    if (now.getTime() > item.expiry) {
        localStorage.removeItem(key);  // Clean up expired item
        return null;
    }

    return item.value;
}
// Retrieve data with expiration check from local storage
function getLocalStorage(key) {
    const itemStr = localStorage.getItem(key);
    if (!itemStr) {
        return null;
    }

    const item = JSON.parse(itemStr);
    const now = new Date();

    // Check if the item is expired
    if (now.getTime() > item.expiry) {
        localStorage.removeItem(key);  // Clean up expired item
        return null;
    }

    return item;
}
function clearLocalStorage() {
    localStorage.clear();
}

