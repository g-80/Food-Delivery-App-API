# AWS Secrets Manager for Database Credentials

# Create secret for database credentials
resource "aws_secretsmanager_secret" "db_credentials" {
  name_prefix             = "${var.project_name}-db-credentials-"
  description             = "Database credentials for ${var.project_name}"
  recovery_window_in_days = 7

  tags = {
    Name = "${var.project_name}-db-credentials"
  }
}

# Store the database credentials
resource "aws_secretsmanager_secret_version" "db_credentials" {
  secret_id = aws_secretsmanager_secret.db_credentials.id
  secret_string = jsonencode({
    engine   = "postgres"
    connection_string = "Host=${aws_db_instance.postgres.address};Port=${aws_db_instance.postgres.port};Username=${var.db_username};Password=${var.db_password};Database=${var.db_name}"
    redis_connection_string = "${aws_elasticache_replication_group.redis.primary_endpoint_address}:${aws_elasticache_replication_group.redis.port}"
  })
}
# IAM policy to allow ECS task to read the secret
resource "aws_iam_role_policy" "ecs_task_execution_secrets" {
  name = "${var.project_name}-ecs-secrets-policy"
  role = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue",
          "kms:Decrypt"
        ]
        Resource = [
          aws_secretsmanager_secret.db_credentials.arn,
          "${aws_secretsmanager_secret.db_credentials.arn}:*"
        ]
      }
    ]
  })
}

resource "aws_secretsmanager_secret" "app_secrets" {
  count                   = 1
  name_prefix             = "${var.project_name}-app-secrets-"
  description             = "Application secrets for ${var.project_name}"
  recovery_window_in_days = 7

  tags = {
    Name = "${var.project_name}-app-secrets"
  }
}

# Store application secrets (API keys, tokens, etc.)
resource "aws_secretsmanager_secret_version" "app_secrets" {
  count     = 1
  secret_id = aws_secretsmanager_secret.app_secrets[0].id
  secret_string = jsonencode({
    jwt_signing_key  = var.app_jwt_signing_key
    stripe_secret_key  = var.app_stripe_secret_key
    mapbox_access_token  = var.app_mapbox_access_token
  })
}

# IAM policy for application secrets
resource "aws_iam_role_policy" "ecs_task_app_secrets" {
  count = 1
  name  = "${var.project_name}-ecs-app-secrets-policy"
  role  = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue",
          "kms:Decrypt"
        ]
        Resource = [
          aws_secretsmanager_secret.app_secrets[0].arn,
          "${aws_secretsmanager_secret.app_secrets[0].arn}:*"
        ]
      }
    ]
  })
}
