output "rds_endpoint" {
  description = "RDS instance endpoint"
  value       = aws_db_instance.postgres.endpoint
  sensitive   = true
}

output "rds_address" {
  description = "RDS instance address"
  value       = aws_db_instance.postgres.address
  sensitive   = true
}

output "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  value       = aws_ecs_cluster.main.name
}

output "cloudwatch_log_group" {
  description = "CloudWatch log group name"
  value       = aws_cloudwatch_log_group.app.name
}

output "db_credentials_arn" {
  description = "ARN of the database credentials secret in Secrets Manager"
  value       = aws_secretsmanager_secret.db_credentials.arn
  sensitive   = true
}

output "app_secrets_arn" {
  description = "ARN of the application secrets in Secrets Manager"
  value       =  aws_secretsmanager_secret.app_secrets[0].arn
  sensitive   = true
}

output "ecr_repository_url" {
  value = aws_ecr_repository.app.repository_url
}