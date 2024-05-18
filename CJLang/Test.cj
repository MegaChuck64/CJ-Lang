void main:
	new str() -> userInput
	new str() -> name

	while (userInput != "ok"):	
		clear()
		input("Enter your name: ") -> name
		input("Enter your age: ") -> userInput
		new u8(userInput) -> age
		welcome_message(name, age) -> userInput

		print(userInput)
		input("Type 'ok' to accept or press enter to try again...") -> userInput


	while (userInput != "exit")
		clear()
		input("Enter a number: ")->userInput
		new i32(userInput)->a
		input("Enter another number: ")->userInput
		new i32(userInput)->b
		add_test(a, b)->a
		print("The sum of the two numbers is: ", a)
		input("Type 'exit' key to exit or enter to try again...")->userInput

	print("")
	print("Goodbye!")


exception main: str errorMessage
	print("main(): ", errorMessage)



//ADD TEST 
i32 add_test: i32 a, i32 b
	set i32(a + b) -> a
	return a

exception add_test: str errorMessage
	print("add_test(a, b): ", errorMessage)



//WELCOME MESSAGE
str welcome_message: str name, u8 age
	str_concat("\nName: ", namev, "\nAge: ", age)->name
	return name


exception welcome_message : str errorMessage
	if (age > 120)
		throw("welcome_message(name, age): ", "Invalid age")
	elif (age < 0)
		throw("welcome_message(name, age): ", "Invalid age")
	elif (name == "")
		throw("welcome_message(name, age): ", errorMessage)
	else
		return errorMessage