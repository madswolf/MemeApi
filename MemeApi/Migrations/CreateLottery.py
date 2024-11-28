import requests
from io import BytesIO
import os

def load_picture(file_path):
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"File not found: {file_path}")
    return open(file_path, "rb")


if __name__ == "__main__":
    base_url = "http://localhost:5001"
    lottery_name = "gamba"
    ticket_cost = 1
    bracket_list = [
        ("1", 1),
        ("2", 1),
        ("3", 1),
        ("4", 1),
    ]

    items_per_bracket = [
        [
            ("1", 5, "1.jpg"),
        ],
        [
            ("2", 5, "2.jpg"),
        ],
        [
            ("3", 5, "3.jpg"),
        ],
        [
            ("4", 5, "4.jpg"),
        ],
    ]



    brackets = [{"BracketName":pair[0], "ProbabilityWeight":pair[1] } for pair in bracket_list]

    lottery_data = {
        "LotteryName": lottery_name,
        "TicketCost": ticket_cost,
        "Brackets": brackets
    }

    headers = {
        'Bot_Secret': ""
    }
    
    lottery_response = requests.post(
        f"{base_url}/Lotteries",
        headers=headers,
        json=lottery_data
    )
    
    
    if lottery_response.status_code == 200:
        print("Lottery created successfully!")
        print(lottery_response.json())
    else:
        print(f"Failed to create lottery: {lottery_response.status_code} - {lottery_response.text}")
        quit()
    
    lottery = lottery_response.json()

    bracket_ids = [bracket["id"] for bracket in lottery["brackets"]] 
    
    if len(bracket_ids) != len(items_per_bracket):
        print("Mismatch between number of brackets and number of item lists!")
        quit()
    for bracket_id, items in zip(bracket_ids, items_per_bracket):
        print(f"Adding items to bracket {bracket_id}...")
        for item in items:
            item_data = {
                "ItemName": item[0],
                "ItemCount": item[1]
            }
            files = {
                "ItemThumbnail": (item[2], load_picture(item[2]), "image/jpeg")
            }

            # Post the item to the AddLotteryItem endpoint
            add_response = requests.post(
                f"{base_url}/lotteries/{bracket_id}/items",
                data=item_data,
                files=files,
                headers=headers
            )
            if add_response.status_code == 200:
                 print("Lottery item added successfully!")
                 print(add_response.json())
            else:
                print(f"Failed to add lottery item: {add_response.status_code} - {add_response.text}")
                quit()
    
    status = 1

    set_status_response = requests.post(
        f"{base_url}/lotteries/{lottery['id']}/SetStatus",
        json=status,
        headers=headers
    )

    if set_status_response.status_code == 200:
         print("Lottery opened successfully!")
    else:
        print(f"Failed to add lottery item: {set_status_response.status_code} - {set_status_response.text}")
        quit()
