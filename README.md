
<div align="center">
    <img width="200px" src="https://www.pngplay.com/wp-content/uploads/7/Cloud-Server-PNG-Clipart-Background.png" alt="server">
    <h1>WebServer</h1>
    <h3>A light Web Server for quick hosting</h3>
    <p>This project is a simple web server implementation using .NET. It was created to study how a web server works and to gain hands-on experience with the .NET framework reguarding ASP.NET. The ultimate goal is to transform this project into a .NET template for creating lightweight servers that can be easily deployed.</p>

<img width="600px" src="./images/Screenshot from 2024-07-15 20-00-43.png">
<hr/>
</div>

## Features ✨

- Serves static files (HTML, CSS, JavaScript, images)
- Routes requests based on file extensions
- Handles default document (e.g., `index.html` for root requests)
- Basic logging using Microsoft.Extensions.Logging

## Getting Started 🚀

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine
- A code editor like [Visual Studio Code](https://code.visualstudio.com/)

### Running the Server

1. **Clone the Repository**
```bash
$ git clone https://github.com/yourusername/webserver-project.git && cd webserver-project
```

2. **Run the Server**
```bash
$ dotnet run
```

The server will start and listen on a non-privileged port (e.g., 6001). You can access it by navigating to http://localhost:6001 in your web browser.

Directory Structure

    `Webserver/`
        `Models/`
            Contains models used by the server
        `wwwroot/`
            Contains static files to be served (HTML, CSS, JS, images)
        `Program.cs`
            Entry point of the application
        `Server/`
            `Server.cs`
                Contains the server logic
            `Router.cs`
                Contains the routing logic

Sample HTML

Here is a sample HTML file (index.html) to be placed in the wwwroot/ directory:

``` html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Document</title>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <div class="container">
        <h1>Hi I'm</h1>
        <h1 class="highlight">Simon Nyvall</h1>
        <h1>I'm am a developer</h1>
        <p class="paragraph ">
    Currently, I'm not employed, but in the mean time I indulge coding my passion projects 📈. My primary goal is to become the best programmer I can be 💻. Simply beacuse I enjoy problem solving.
        </p>
        <div class="buttons">
            <a href="https://github.com/SimonNyvall/WebServer" class="button github">Github</a>
            <a href="" class="button" id="btn">Shout Hello World!</a>
        </div>
    <div>

    <script src="app.js"></script>
</body>
</html>
```

## Future Plans 📈
- Transform the project into a .NET template
- Add support for dynamic content
- Implement more advanced routing
- Add middleware support (e.g., for authentication)
- Improve error handling

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue if you have any suggestions or find any bugs.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.