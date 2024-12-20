import re
import sys
import democrite
import lxml.html
import urllib.request

def extract_article_content(command: democrite.vgrainCommand, tools: democrite.vgrainTools):
    """ Eval and calculate the mathematical result """
    html_page_url = command.get_content()

    content = ""

    if html_page_url is None:
        tools.get_logger.logWarning("null request");
        return {
            "Content" : "",
            "Source" : ""
        }
    
    try:    
        with urllib.request.urlopen(html_page_url) as f:
            html = f.read().decode('utf-8')

        t = lxml.html.fromstring(html)
        content = t.text_content()

        # Clean
        content = re.sub(r'\n[\n\s]+', '##SEPARATOR_PART##', content)
        parts = content.split('##SEPARATOR_PART##')

        cleanContent = []
    
        for msg in parts:

            msg = msg.strip()
            size = len(msg)
            if "function(" in msg or size < 50 or msg.count(' ') < 15:
                continue

            cleanContent.append(msg)

        content = "\n".join(cleanContent)

        tools.get_logger.logInformation("Source = '" + html_page_url + "' Result: " + content)

    except Exception as ex:
        tools.get_logger.logError("Source = '" + html_page_url + "' Error : " + str(ex))
        content = str(ex)

    content = ascii(content).strip('"').strip("'")
    content = re.sub(r'[\]{1}[x]{1}[a-fA-F0-9]+[0]{1}', ' ', content)
    content = re.sub(r'[\]{1}[u]{1}[a-fA-F0-9]+[0]{1}', ' ', content)
    content = content.replace('"', '\\"').replace("'", "\\\'").replace("\\ ", " ")

    return {
        "Content" : content,
        "Source" : html_page_url
    }
        

app = democrite.vgrain(sys.argv)
app.run(extract_article_content)

# app.test("https://news.mit.edu/2024/ai-health-should-be-regulated-dont-forget-about-algorithms-1212", extract_article_content);