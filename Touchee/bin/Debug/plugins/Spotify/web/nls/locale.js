define({root:{
  
  spotify: {
    spotify: "Spotify",

    config: {
      session: {
        title:      "Account",
        login:      "Login",
        logout:     "Logout",

        loggingIn:    "Logging in...",
        offline:      "Connecting...",
        loggedIn:     "Logged in as <em>%s</em>",
        disconnected: "Disconnected, reconnecting...",
        
        error:     {
          400:  "Undefined error",
          401:  "Wrong username / password",
          422:  "Enter username and password"
        }
      }
    }
  },

  attributes: {
    spotify_session: {
      username: "username",
      password: "password"
    }
  }
  
}});