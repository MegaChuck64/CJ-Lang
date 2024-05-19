class Player: 
    u8 Age
    str Name
    u8 Health

    ctor(u8 age, str name, u8 health):
        Age = age
        Name = name
        Health = health

    str get_info():
        new str() -> msg
        str_concat("\nName: ", Name, "\nAge: ", Age, "\nHealth: ", Health) -> msg
        return msg

    void take_damage(u8 damage):
        set u8(Health - damage) -> Health

        