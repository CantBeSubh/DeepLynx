#!/bin/bash

copy_file() {
    local sourceFilePath="$1"
    local destinationDirectory="$2"

    mkdir -p "$destinationDirectory"

    local fileName=$(basename "$sourceFilePath")
    local destinationFilePath="$destinationDirectory/$fileName"

    if [ -e "$destinationFilePath" ]; then
        echo "File '$fileName' already exists in '$destinationDirectory'. Skipping copy."
        return
    fi

    cp "$sourceFilePath" "$destinationFilePath"
    echo "File '$fileName' copied to '$destinationDirectory'."
}

add_to_bashrc() {
    local variable="$1"
    local value="$2"

    local bashrcPath="$HOME/.bashrc"
    local bashProfilePath="$HOME/.bash_profile"
    local zshrc="$HOME/.zshrc"
    local lineToAdd="export $variable=$value:\$$variable"

    if [ -e "$bashrcPath" ]; then
        if ! grep -qF "$lineToAdd" "$bashrcPath"; then
            echo -e "\n$lineToAdd" >> "$bashrcPath"
            echo "Added '$lineToAdd' to $bashrcPath."
        else
            echo "The line '$lineToAdd' already exists in $bashrcPath."
        fi
    fi

    if [ -e "$bashProfilePath" ]; then
        if ! grep -qF "$lineToAdd" "$bashProfilePath"; then
            echo -e  "\n$lineToAdd" >> "$bashProfilePath"
            echo "Added '$lineToAdd' to $bashProfilePath."
        else
            echo "The line '$lineToAdd' already exists in $bashProfilePath."
        fi
    fi
    
    if [ -e "$zshrc" ]; then
        if ! grep -qF "$lineToAdd" "$zshrc"; then
            echo -e  "\n$lineToAdd" >> "$zshrc"
            echo "Added '$lineToAdd' to $zshrc."
        else
            echo "The line '$lineToAdd' already exists in $zshrc."
        fi
    fi
}

delete_directory() {
    local targetDir="$1"

    if [ -d "$targetDir" ]; then
        rm -rf "$targetDir"
        echo "Directory '$targetDir' deleted successfully."
    else
        echo "Directory '$targetDir' does not exist."
    fi
}

testsLibraryPath="deeplynx.tests/bin/Debug/net10.0/runtimes/osx/native"
apiLibraryPath="deeplynx.api/bin/Debug/net10.0/runtimes/osx/native"
graphLibraryPath="deeplynx.graph/bin/Debug/net10.0/runtimes/osx/native"

if [ -d "$testsLibraryPath" ]; then
    testsAbsolutePath=$(cd "$testsLibraryPath" && pwd)
else
    echo "Error: The directory does not exist."
    exit 1
fi

if [ -z "$testsAbsolutePath" ]; then
    echo "Error: The absolute path is empty."
    exit 1
fi

echo "Tests Absolute Path: $testsAbsolutePath"


if [ -d "$apiLibraryPath" ]; then
    apiAbsolutePath=$(cd "$apiLibraryPath" && pwd)
else
    echo "Error: The directory does not exist."
    exit 1
fi

if [ -z "$apiAbsolutePath" ]; then
    echo "Error: The absolute path is empty."
    exit 1
fi

echo "Api Absolute Path: $apiAbsolutePath"

kuzuFilesDirectory="deeplynx.graph/KuzuFiles"
libkuzunetFilePath="deeplynx.graph/KuzuFiles/libkuzunet.dylib"
libkuzuFilePath="deeplynx.graph/KuzuFiles/libkuzu.dylib"
testsDestinationDirectory="$testsLibraryPath"
apiDestinationDirectory="$apiLibraryPath"

copy_file "$libkuzunetFilePath" "$testsDestinationDirectory"
copy_file "$libkuzuFilePath" "$testsDestinationDirectory"

copy_file "$libkuzunetFilePath" "$apiDestinationDirectory"
copy_file "$libkuzuFilePath" "$apiDestinationDirectory"

add_to_bashrc "DYLD_LIBRARY_PATH" "$testsAbsolutePath"
add_to_bashrc "DYLD_LIBRARY_PATH" "$apiAbsolutePath"

export DYLD_LIBRARY_PATH="$testsAbsolutePath:$DYLD_LIBRARY_PATH"
export DYLD_LIBRARY_PATH="$apiAbsolutePath:$DYLD_LIBRARY_PATH"

echo "DYLD_LIBRARY_PATH set to: $DYLD_LIBRARY_PATH"
