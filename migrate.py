import json
import time
import requests

# Opening JSON file
f = open('memes.json', encoding='utf-8')
f_visual = open('visuals.json', encoding='utf-8')
f_toptext = open('toptexts.json', encoding='utf-8')
f_bottomtext = open('bottomtexts.json', encoding='utf-8')
# returns JSON object as 
# a dictionary
data = json.load(f)
data_visuals = json.load(f_visual)
data_toptexts = json.load(f_toptext)
data_bottomtexts = json.load(f_bottomtext)
mediaurl = "https://media.mads.monster/visual/"
meme_url = "https://localhost/memes"
text_url = "https://localhost/texts"
visual_url = "https://localhost/visuals"
# Iterating through the json
# list

meme_ids = []
visuals_ids = []
toptext_ids = []
bottomtext_ids = []
for meme in data:
    visual = meme["visual"]
    if(not visual):
        #Not an actual meme
        continue

    visual_filename = visual["filename"]
    
    visuals_ids.append(visual["id"])
    visual_file = requests.get(mediaurl + visual_filename, allow_redirects=True)
    meme_ids.append(meme["id"])

    toptext = meme["topText"]
    if(toptext):
        toptext_ids.append(toptext["id"])
        toptext = toptext["memetext"]

    bottomtext = meme["bottomText"]
    if(bottomtext):
        bottomtext_ids.append(bottomtext["id"])
        bottomtext = bottomtext["memetext"]

    data = {"Toptext": toptext,"Bottomtext": bottomtext, "FileName":visual_filename}
    session = requests.Session()
    session.verify = False
    response = session.post(meme_url,files = {"VisualFile": visual_file.content}, data = data)
    if(not response.ok):
        print(visual_file, response)

  
for visual in data_visuals:
    if visual["id"] not in visuals_ids:
        session = requests.Session()
        session.verify = False
        visual_file = requests.get(mediaurl + visual["filename"], allow_redirects=True)
        data = {"Toptext": "","Bottomtext": "", "FileName":visual["filename"]}
        response = session.post(meme_url,files = {"VisualFile": visual_file.content}, data = data)
        if(not response.ok):
            print(visual, response)

for toptext in data_toptexts:
    if toptext["id"] not in toptext_ids:
        session = requests.Session()
        session.verify = False
        response = session.post(text_url, data = json.dumps({"Text":toptext["memetext"], "Position": "TopText"}), headers={'content-type': 'application/json'})
        if(not response.ok):
            print(toptext, response)

for bottomtext in data_bottomtexts:
    if bottomtext["id"] not in bottomtext_ids:
        session = requests.Session()
        session.verify = False
        response = session.post(text_url, data = json.dumps({"Text":bottomtext["memetext"], "Position": "BottomText"}), headers={'content-type': 'application/json'})
        if(not response.ok):
            print(bottomtext, response)


print(len(visuals_ids))
print(len(meme_ids))
print(len(toptext_ids))
print(len(bottomtext_ids))
# Closing file
f.close()