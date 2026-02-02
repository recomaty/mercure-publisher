Feature: Configure Mercure publisher options
  As a developer
  I want the publisher options to be validated
  So that configuration errors are caught early

  @validation
  Scenario: Valid configuration passes validation
    Given I configure the publisher with:
      | Option | Value                         |
      | Host   | https://mercure.example.com   |
      | Token  | valid-jwt-token               |
    When I validate the options
    Then the validation should pass

  @validation
  Scenario: Empty host fails validation
    Given I configure the publisher with:
      | Option | Value             |
      | Host   |                   |
      | Token  | valid-jwt-token   |
    When I validate the options
    Then the validation should fail
    And the validation error should mention "Host"

  @validation
  Scenario: Invalid URL format fails validation
    Given I configure the publisher with:
      | Option | Value             |
      | Host   | not-a-valid-url   |
      | Token  | valid-jwt-token   |
    When I validate the options
    Then the validation should fail
    And the validation error should mention "Host"

  @validation
  Scenario: Empty token fails validation
    Given I configure the publisher with:
      | Option | Value                         |
      | Host   | https://mercure.example.com   |
      | Token  |                               |
    When I validate the options
    Then the validation should fail
    And the validation error should mention "Token"

  @validation
  Scenario: Both host and token invalid fails with multiple errors
    Given I configure the publisher with:
      | Option | Value           |
      | Host   | invalid-url     |
      | Token  |                 |
    When I validate the options
    Then the validation should fail
    And the validation error should mention "Host"
    And the validation error should mention "Token"
