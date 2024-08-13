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
meme_url = "http://localhost:5001/memes"
text_url = "http://localhost:5001/texts"
visual_url = "http://localhost:5001/visuals"
# Iterating through the json
# list

meme_ids = []
visuals_filenames = []
toptext_texts = []
bottomtext_texts = []
for meme in data:
    visual_filename = meme["memeVisual"]
    if(not visual_filename):
        #Not an actual meme
        continue

    
    visuals_filenames.append(visual_filename)
    visual_file = requests.get(mediaurl + visual_filename, allow_redirects=True)
    meme_ids.append(meme["id"])

    toptext = meme["toptext"]
    if(toptext):
        toptext_texts.append(toptext)
        toptext = toptext

    bottomtext = meme["bottomText"]
    if(bottomtext):
        bottomtext_texts.append(bottomtext)
        bottomtext = bottomtext

    data = {"Toptext": toptext,"Bottomtext": bottomtext, "FileName":visual_filename, "Topics":meme["topics"], "CreatedAt":meme["CreatedAt"]}
    session = requests.Session()
    session.verify = False
    response = session.post(meme_url,files = {"VisualFile": visual_file.content}, data = data)
    if(not response.ok):
        print(visual_file, response)
    else:
        print("okay", meme["id"])

  
for visual in data_visuals:
    if visual["id"] not in visuals_filenames:
        session = requests.Session()
        session.verify = False
        visual_file = requests.get(mediaurl + visual["filename"], allow_redirects=True)
        data = {"Toptext": "","Bottomtext": "", "FileName":visual["filename"], "Topics":visual["topics"], "CreatedAt":visual["CreatedAt"]}
        response = session.post(meme_url,files = {"VisualFile": visual_file.content}, data = data)
        if(not response.ok):
            print(visual, response)
        else:
            print("okay visual: ", visual)

for toptext in data_toptexts:
    if toptext not in toptext_texts:
        session = requests.Session()
        session.verify = False
        response = session.post(text_url, data = json.dumps({"Text":toptext["text"], "Position": "TopText", "Topics":toptext["topics"], "CreatedAt":toptext["CreatedAt"]}), headers={'content-type': 'application/json'})
        if(not response.ok):
            print(toptext, response)
        else:
            print("okay toptext: ", toptext)

for bottomtext in data_bottomtexts:
    if bottomtext not in bottomtext_texts:
        session = requests.Session()
        session.verify = False
        response = session.post(text_url, data = json.dumps({"Text":bottomtext["text"], "Position": "BottomText", "Topics":bottomtext["topics"], "CreatedAt":toptext["CreatedAt"]}), headers={'content-type': 'application/json'})
        if(not response.ok):
            print(bottomtext, response)
        else:
            print("okay bottomtext: ", bottomtext)    


print(len(visuals_filenames))
print(len(meme_ids))
print(len(toptext_texts))
print(len(bottomtext_texts))
# Closing file
f.close()