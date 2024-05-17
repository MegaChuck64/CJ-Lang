void main:
	new str() -> userInput
	
	input("enter name") -> userInput	
	new str(userInput) -> userName
	
	input("enter age") -> userInput
	new str(userInput) -> userAge
	
	//can parse a str in the constructor
	new u8(userAge) -> age
	
	clear()

	print("Hello World...")
	print(">>>>>>>>")
	new str() -> msg
	str_concat("Welcome, ", userName) -> msg
	print(msg)
	welcome_message(userName, age) -> msg
	print(msg)

	if (age > 18) :
		print("You can vote ", userName, ".")
	elif (age < 18) :
		print("Sorry, ", userName, ". You can't vote.")
	else :
		print("Congrats, ", userName, ". You can vote in your first election.")

	while (age < 18) :
		print("You are not old enough to vote.")
		set u8(age + 1) -> age
		print("You are now ", age, " years old.")


	new i32(10) -> a
	new i32(20) -> b
	new i32() -> c
	add_test(a, b) -> c
	print("a + b = ", c)

	print("Goodbye, ", userName, ".")


	input("Press enter key to exit...")


exception main: str errorMessage
	print("An Error Occured: ", errorMessage)



//ADD TEST 
i32 add_test: i32 a, i32 b
	set i32(a + b) -> a
	return a

exception add_test: str errorMessage
	print("add_test error: ", errorMessage)



//WELCOME MESSAGE
str welcome_message: str name, u8 age
	str_concat("\nName: ", name, "\nAge: ", age)->name
	return name
