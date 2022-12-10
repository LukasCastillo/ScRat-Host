# ScRat-Host
The host server for the scuffed windows remote access tool ScRat

Features:
- Remote Shell
- Remote Screenshot
- Uploading and Downloading files

## Usage
```
setid [client id or "all"]            - sets the current client to communicate to. If this is set to "all" it will communicate to all connected clients all at once.
getid                                 - prints all connected clients and some info.
ssh [command]                         - runs the cmd command on the current client.
download [path]                       - downloads the file specified by the path from the current client.
upload [sourcePath] [destinationPath] - transfers the file in the host computer to the current client.
screenshot [width] [height]           - takes a screenshot of the current client's screen.
exit                                  - terminates the current client ScRat process.
stop                                  - stops the ScRat server.
```

Files that are downloaded from commands like "download" and "screenshot" will be separated into different files by the clients computer name.

![image](https://user-images.githubusercontent.com/112668532/206861238-3bb53ce4-bf3b-4165-bc23-a47f02ee9db2.png)

## ScRat Client Repo
https://github.com/LukasCastillo/ScRat
