# Conversation Composer for Voice SDK

Voice SDK provides support for Composer, a new feature in Wit.ai for designing interactive conversations. Composer is a graph-based dialogue designer that can understand Intents, trigger client-side actions, and provide static or dynamic responses based upon the flow of the conversation.

Data received from intents and entities is stored in a key-value JSON object, the `context_map`. From the client side, this context map can receive values from Composer as well as be updated to provide data to Composer.

### Composer consists of four types of modules:
* Input modules, which represent information that comes from your end user or from another system.
* Response modules, which provide the user with a text or voice response, or trigger a client-side action.
* Decision modules, which provide logic to control the flow of the conversation.
* Context modules, which modify the data in the context map.

### Further documentation
* For the web interface of Wit.ai, we provide a number of recipes that will give you additional information about how to use Composer, at https://wit.ai/docs/recipes
* For the Unity integration with Composer, please see our resources online at https://developer.oculus.com/documentation/unity/voice-sdk-composer-creating/

## Dependencies
This package requires the core Voice SDK package, which will install automatically when installing this package from the Unity Asset Store.
