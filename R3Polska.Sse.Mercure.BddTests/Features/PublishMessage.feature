Feature: Publish messages to Mercure hub
  As a developer
  I want to publish messages to a Mercure hub
  So that subscribers can receive real-time updates

  Background:
    Given a Mercure hub is configured at "https://mercure.example.com"
    And the JWT token is "test-jwt-token"

  @happy-path
  Scenario: Successfully publish a message with topic and payload
    Given I have a message with topic "orders/123"
    And the payload contains:
      | Field    | Value          |
      | OrderId  | order-456      |
      | Status   | created        |
    When I publish the message
    Then the message should be sent successfully
    And the request should be sent to "https://mercure.example.com/.well-known/mercure"
    And the Authorization header should be "Bearer test-jwt-token"

  @happy-path
  Scenario: Publish a message with a custom ID
    Given I have a message with topic "notifications/user-1"
    And the message ID is "msg-custom-123"
    And the payload contains:
      | Field   | Value              |
      | Message | Hello, World!      |
    When I publish the message
    Then the message should be sent successfully
    And the request body should contain "id=msg-custom-123"

  @happy-path
  Scenario: Publish a message without an ID
    Given I have a message with topic "events/system"
    And the payload contains:
      | Field | Value        |
      | Event | system-start |
    When I publish the message
    Then the message should be sent successfully
    And the request body should not contain "id="

  @happy-path
  Scenario: Payload is serialized as JSON
    Given I have a message with topic "data/complex"
    And the payload contains:
      | Field  | Value     |
      | Name   | Test Item |
      | Count  | 42        |
    When I publish the message
    Then the message should be sent successfully
    And the request body should contain the JSON payload
