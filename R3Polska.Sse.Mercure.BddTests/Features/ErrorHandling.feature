Feature: Handle publishing errors
  As a developer
  I want to receive meaningful errors when publishing fails
  So that I can handle failures appropriately

  Background:
    Given a Mercure hub is configured at "https://mercure.example.com"
    And the JWT token is "test-jwt-token"

  @error-handling
  Scenario Outline: Server returns an error status code
    Given I have a message with topic "test/topic"
    And the payload contains:
      | Field | Value |
      | Data  | test  |
    And the Mercure hub will respond with status <StatusCode> and message "<ErrorMessage>"
    When I attempt to publish the message
    Then a MercurePublisherException should be thrown
    And the exception message should contain "<StatusName>"
    And the exception message should contain "<ErrorMessage>"

    Examples:
      | StatusCode | StatusName          | ErrorMessage           |
      | 400        | BadRequest          | Bad Request            |
      | 401        | Unauthorized        | Unauthorized           |
      | 403        | Forbidden           | Forbidden              |
      | 500        | InternalServerError | Internal Server Error  |

  @error-handling
  Scenario: Network error during publishing
    Given I have a message with topic "test/topic"
    And the payload contains:
      | Field | Value |
      | Data  | test  |
    And the Mercure hub is unreachable
    When I attempt to publish the message
    Then a MercurePublisherException should be thrown
    And the exception should have an inner exception of type "HttpRequestException"

  @error-handling
  Scenario: Request is cancelled
    Given I have a message with topic "test/topic"
    And the payload contains:
      | Field | Value |
      | Data  | test  |
    And the request will be cancelled
    When I attempt to publish the message
    Then a MercurePublisherException should be thrown
    And the exception should have an inner exception of type "TaskCanceledException"
