
void main():
	new str() -> userInput
	new str() -> output
	while (userInput != "exit"):
		clear()
		input("Enter your name: ") -> userInput
		new str(userInput) -> userName

		input("Enter your age: ") -> userInput
		new u8(userInput) -> userAge
		input("confirm? y/n") -> userInput
        
		if (userInput == "y"):
			welcomeMessage(userName, userAge) -> output
			print(output)
			input("Press enter to continue")
			set str("exit") -> userInput
			new Player(userAge, userName, 100) -> player
			player.get_info() -> output
			print(output)

		else:
			input("Press enter to continue")

str welcomeMessage(str name, u8 age):
	str_concat("\nName: ", name, "\nAge: ", age) -> name
	return name

//todo: 
//  need to convert to classes
//  multiple files
//  link external libs
//  arrays
//  for/foreach loops
//  io functions (file read/write) 

