# NarrAItor

A Modern Text Adventure Game

Used in this project:

1. Blazor WebAssembly - I wanted the game to feel responsive, just like those old text adventures. Server-side rendering would introduce latency that would break the immersion. WebAssembly lets me handle all the game logic client-side, with the server only getting involved for AI operations.

2. Tailwind CSS - Getting that retro look while keeping things modern required some careful styling. Tailwind's utility-first approach made it easy to experiment with different looks until I found the perfect balance between nostalgia and usability.

3. Command Pattern - This was crucial for handling player input in a maintainable way. It lets me add new commands without touching existing code and makes testing much easier.

4. OpenAI Integration - This is what makes each playthrough unique. Though I have to admit, getting it to behave was trickier than I expected.

For more information see the blog article at: https://tjgokken.com/building-a-modern-text-adventure-game
