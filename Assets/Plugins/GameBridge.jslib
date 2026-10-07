// Funciones que Unity llama en la página de juego. Cada una llama a window.hordeGame, que publica
// el wrapper de horde (packages/game-wrapper), y no hace nada si todavía no existe.
mergeInto(LibraryManager.library, {
  ConnectToGameApi: function () {
    if (window.hordeGame) window.hordeGame.connect();
  },

  CheckForInitialStatus: function () {
    if (window.hordeGame) window.hordeGame.sceneReady();
  },

  SendGameInput: function (path) {
    var text = UTF8ToString(path);
    if (window.hordeGame) window.hordeGame.sendPath(text);
  },

  ShowUIControls: function () {
    if (window.hordeGame) window.hordeGame.showControls();
  },

  HideUIControls: function () {
    if (window.hordeGame) window.hordeGame.hideControls();
  },

  ChangeCanvasOpacity: function () {
    if (window.hordeGame) window.hordeGame.revealCanvas();
  },

  CloseSplash: function () {
    if (window.hordeGame) window.hordeGame.closeSplash();
  },

  CloseGame: function () {
    if (window.hordeGame) window.hordeGame.finished();
  }
});
