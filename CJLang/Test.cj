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
	print(-)
	new str() -> msg
	str_concat("Welcome, ", userName, "\nName: ", userName, "\nAge: ", age)->msg

	print(msg)

	if (age > 18) :
		print("You can vote")
	elif (age < 18) :
		print("You can't vote")
	else :
		print("You can vote in your first election")

	input("Press enter key to exit...")


main: str errorMessage -> exception
	new str() -> msg
	str_concat("An error occured: ", errorMessage) -> msg
	print(msg)
