window.updateAvailable = Promise.resolve(false);
window.registerForUpdateAvailableNotification = () => {};
window.appUpdate = {
  checkForUpdates: async () => ({ status: 'Unsupported' }),
  applyUpdate: async () => {},
  subscribe: () => {},
  unsubscribe: () => {}
};
