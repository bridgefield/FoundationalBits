# FoundationalBits

[![CI](https://github.com/bridgefield/FoundationalBits/actions/workflows/ci.yml/badge.svg)](https://github.com/bridgefield/FoundationalBits/actions/workflows/ci.yml) [![CodeQL](https://github.com/bridgefield/FoundationalBits/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/bridgefield/FoundationalBits/actions/workflows/codeql-analysis.yml) [![Nuget](https://img.shields.io/nuget/v/bridgefield.FoundationalBits)](https://www.nuget.org/packages/bridgefield.FoundationalBits/)

Useful bits for foundational groundwork in C# applications.

## Contents

- MessageBus for async dispatch of messages between components in the same
  process
  ```csharp 
  
  class Receiver : IHandle<string>{
    public void Handle(string message) =>
      Console.WriteLine(message);
  }
  
  ...
  
  var messageBus = new AgentBasedMessageBus()
  messageBus.Subscribe(
    new Receiver(),
    SubscriptionLifecycle.ExplicitUnsubscribe);
  
  ...
  
  await messageBus.Publish("Hello, World!");
  
  ```
- Stateful agent to handle shared mutable state without having to use explicit
  lock statements or other semaphores
  ```csharp
  enum CounterCommand{
    Increment,
    Decrement,
    Current,
  }
  var counter = IAgent<CounterCommand,int>.Start<int>(
    0,
    (current,command) => command switch{
      CounterCommand.Increment => (current+1, current+1),
      CounterCommand.Decrement => (current-1, current-1),
      CounterCommand.Current => (current,current),
      _=>throw new ArgumentException("unknown command")
    }
  )
    
  ... 
  //thread/task 1
    
  await counter.Tell(CounterCommand.Increment);
    
  //thread/task 2
    
  await counter.Tell(CounterCommand.Decrement);
  
  ...
  //output: "0" regardless of ordering of previous calls
  Console.WriteLine(await counter.Tell(CounterCommand.Current));
  ```
- state less agent to ensure generalized sequential execution of actions
  triggered from different background tasks without explicit synchronization

## Contributors

- Andreas Pfohl
- Dirk Peters
