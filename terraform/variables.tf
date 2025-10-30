variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
  default     = "eu-west-2"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "food-delivery-app-demo"
}

variable "image_tag" {
  description = "Docker image tag"
  type        = string
}

variable "container_port" {
  description = "Port exposed by the container"
  type        = number
  default     = 8080
}

variable "health_check_path" {
  description = "Path for ALB health checks"
  type        = string
  default     = "/health"
}

variable "db_name" {
  description = "Database name"
  type        = string
  default     = "appdb"
}

variable "db_username" {
  description = "Database master username"
  type        = string
  default     = "dbadmin"
  sensitive   = true
}

variable "db_password" {
  description = "Database master password"
  type        = string
  sensitive   = true
}

variable "log_retention_days" {
  description = "CloudWatch log retention in days"
  type        = number
  default     = 7
}

# Auto Scaling Configuration
variable "asg_desired_capacity" {
  description = "Desired number of EC2 instances in the Auto Scaling Group"
  type        = number
  default     = 1
}

variable "asg_min_size" {
  description = "Minimum number of EC2 instances in the Auto Scaling Group"
  type        = number
  default     = 1
}

variable "asg_max_size" {
  description = "Maximum number of EC2 instances in the Auto Scaling Group"
  type        = number
  default     = 3
}

variable "ecs_service_desired_count" {
  description = "Desired number of ECS tasks"
  type        = number
  default     = 1
}

variable "ecs_service_min_capacity" {
  description = "Minimum number of ECS tasks for auto scaling"
  type        = number
  default     = 1
}

variable "ecs_service_max_capacity" {
  description = "Maximum number of ECS tasks for auto scaling"
  type        = number
  default     = 4
}

variable "ecs_cpu_target" {
  description = "Target CPU utilization percentage for ECS service auto scaling"
  type        = number
  default     = 80
}

variable "ecs_memory_target" {
  description = "Target memory utilization percentage for ECS service auto scaling"
  type        = number
  default     = 80
}

# Secrets Manager Configuration
variable "app_stripe_secret_key" {
  type        = string
  sensitive   = true
}

variable "app_jwt_signing_key" {
  type        = string
  sensitive   = true
}

variable "app_mapbox_access_token" {
  type        = string
  sensitive   = true
}

# Application Configuration
variable "app_environment" {
  description = "Application environment (development, staging, production)"
  type        = string
  default     = "production"
}

variable "log_level" {
  description = "Application log level"
  type        = string
  default     = "info"
}
