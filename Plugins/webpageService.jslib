mergeInto(LibraryManager.library, {

  SendMessageToBrowser: function (message) {
    MessageReceivedFromUnity(Pointer_stringify(message));
  },

  WebGLStartGame: function() {
    GameInitialized();
  }

});