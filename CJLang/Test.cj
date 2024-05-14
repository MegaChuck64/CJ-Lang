main: -> void
	new str() -> userInput
	
	input("enter name") -> userInput	
	new str(userInput) -> userName
	
	input("enter age") -> userInput
	new str(userInput) -> userAge
	
	//can parse a str in the constructor
	new i16(userAge) -> age
	print(age)

	set i16(50) -> age
	print(age)


	print("Hello World...")
	
	new str() -> msg
	str_concat("Welcome, ", userName, "\nName: ", userName, "\nAge: ", userAge)->msg

	print(msg)


main: str errorMessage -> exception
	new str() -> msg
	str_concat("An error occured: ", errorMessage) -> msg
	print(msg)
