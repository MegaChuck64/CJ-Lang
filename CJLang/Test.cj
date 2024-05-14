main: -> void
	new str() -> userInput
	
	input("enter name") -> userInput	
	new str(userInput) -> userName
	
	input("enter age") -> userInput
	new str(userInput) -> userAge
	
	//can parse a str in the constructor
	new u8(userAge) -> age
	
	clear()

	print("Hello World...")
	
	new str() -> msg
	str_concat("Welcome, ", userName, "\nName: ", userName, "\nAge: ", age)->msg

	print(msg)

	input("Press any key to exit...")


main: str errorMessage -> exception
	new str() -> msg
	str_concat("An error occured: ", errorMessage) -> msg
	print(msg)
