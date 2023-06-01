using UnityEngine;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class APIConnect : MonoBehaviour
{
    string host = "127.0.0.1:5000";
    string uri;
    string fullPrompt;
    string characterPrompt = "complete character prompt here";

    void Start()
    {
        uri = $"http://{host}/api/v1/generate";
        Debug.Log("connected: " + uri);
    }

    public async Task<string> interact(string prompt)
    {
        string response;
        Debug.Log("Connect to: " + uri + " \nWith prompt: " + prompt + "\nplease wait");
        if (string.IsNullOrWhiteSpace(fullPrompt))
        {
            response = await Run(uri, prompt);
        }
        else
        {
            response = await Run(uri, fullPrompt);
        }
        fullPrompt = fullPrompt + "user:" + prompt + "character:" + response + "\n";
        return response;
    }

    async Task<string> Run(string uri, string prompt)
    {
        var request = new
        {
            prompt = (characterPrompt + "You:" + prompt + "character:"),
            max_new_tokens = 200,
            do_sample = true,
            temperature = 0.5,
            top_p = 0.9,
            typical_p = 1,
            epsilon_cutoff = 0,
            eta_cutoff = 0,
            repetition_penalty = 1.1,
            encoder_repetition_penalty = 1,
            top_k = 0,
            min_length = 0,
            no_repeat_ngram_size = 0,
            num_beams = 1,
            penalty_alpha = 0,
            length_penalty = 1,
            early_stopping = false,
            mirostat_mode = 0,
            mirostat_tau = 5,
            mirostat_eta = 0.1,
            seed = -1,
            add_bos_token = true,
            truncation_length = 2048,
            ban_eos_token = false,
            skip_special_tokens = true,
            mode = "chat",
            model = "gozfarb_pygmalion-7b-4bit-128g-cuda",
            stopping_strings = new string[] { "You:", "<start>" }
        };

        using (HttpClient client = new HttpClient())
        {
            var jsonRequest = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(jsonResponse);
                var textResult = result["results"][0]["text"].ToString();
                Debug.Log(textResult);

                string trimmedResponse = textResult.Trim();
                if (trimmedResponse.EndsWith("You:") || trimmedResponse.EndsWith("You"))
                {
                    int lastYouIndex = trimmedResponse.LastIndexOf("You:");
                    if (lastYouIndex >= 0)
                    {
                        trimmedResponse = trimmedResponse.Substring(0, lastYouIndex).Trim();
                    }
                }

                Debug.Log(textResult + " : " + trimmedResponse);
                return trimmedResponse;
            }
            else
            {
                return "Error: " + response.StatusCode.ToString();
            }
        }
    }
}
