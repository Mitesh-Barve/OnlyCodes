// Import the Express.js framework
const express = require("express"); 
// Express is a web application framework for Node.js that simplifies building web servers and APIs.

// Import the path module
const path = require("path"); 
// The path module provides utilities for working with file and directory paths in a way that is platform-independent.

// Create an instance of the Express application
const app = express();
// This 'app' object will be used to define routes, middleware, and configurations for our server.

// Define the port number where the server will run
const port = 8080; 
// When we start the server, it will listen for incoming requests on port 8080. 
// If another service is using this port, you might need to change it to another available port.

// Middleware to parse incoming requests with URL-encoded payloads
app.use(express.urlencoded({ extended: true }));
// 'express.urlencoded()' is a built-in middleware function in Express. It parses incoming requests 
// with URL-encoded data (typically from HTML forms). 
// The 'extended: true' option allows parsing of nested objects in form data.

// Set EJS as the templating engine
app.set("view engine", "ejs");
// Express supports various template engines like Pug, Handlebars, and EJS. 
// Here, we are setting EJS (Embedded JavaScript) as our template engine, 
// which allows us to generate dynamic HTML content by embedding JavaScript into our templates.

// Set the directory where the EJS template files (views) are stored
app.set("views", path.join(__dirname, "views"));
// '__dirname' represents the absolute path of the directory where this script is located.
// 'path.join(__dirname, "views")' constructs the absolute path to the 'views' folder.
// This ensures that Express looks for EJS files inside the 'views' directory.

// Middleware to serve static files such as CSS, JavaScript, and images
app.use(express.static(path.join(__dirname, "public")));
// 'express.static()' is a built-in middleware function in Express that serves static files from the specified folder.
// Here, we specify the 'public' folder, meaning any file inside 'public' (e.g., CSS, images, JavaScript files) 
// can be accessed directly in the browser without needing a specific route.

// Define a route to handle GET requests for the home ('/') page
app.get("/", (req, res) => {
    res.send("Server is running!"); 
    // When a client (browser) makes a GET request to 'http://localhost:8080/', 
    // the server responds with the text "Server is running!".
    // 'res.send()' sends a simple text response to the client.
});

// Start the server and listen for incoming requests on the defined port
app.listen(port, () => {
    console.log(`Listening on port ${port}`); 
    // When the server starts, this message is printed in the terminal, 
    // indicating that the server is successfully running and listening for requests.
});
