# SaveFile Demo 1

My first attempts at creating tiny .save files

## Usage

### **Normal Mode**

**Normal Mode** stores all program data in the current directory

> ### Run in **Normal Mode**
>
> To open the program in **Normal Mode** just double click the file or run the following command in your Terminal
>
> ```cmd
> savefile_demo_1.exe
> ```

### **Global Mode**

**Global Mode** stores all program data in your LocalAppData under  `henrisen_savefile_demo_1`
To get there you can run the following command: `explorer %localappdata%\henrisen_savefile_demo_1`

> ### Run in **Global Mode**
>
> To open the program in **Global Mode**  run the following command in your Terminal
>
> ```cmd
> savefile_demo_1.exe -g
> ```
