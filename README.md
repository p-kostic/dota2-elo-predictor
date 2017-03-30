# INFOB3OMG
Onderzoeksmethoden voor gametech



Statistic Procedures in R
``` R
#read the file
mydata = read.csv("path", sep = ";")


######## SPLITTING DATA ##############
# split the index to 80% of the sample size
smp_size <- floor(0.80 * nrow(mydata))
# set the seed to make our partition reproductible
set.seed(123)
train_ind <- sample(seq_len(nrow(mydata)), size = smp_size)
# split the data using the index
train <- mydata[train_ind, ]
test <- mydata[-train_ind, ]



######## LOGISTIC REGRESSION #######
# show the active window
windows(title="Elodiff vs. outcome")

#plot the graph
plot(train$elo.diff, train$outcome, xlab="Elo difference", ylab="Probability of winning", xlim=c(-1000,1000),cex.lab=1.5)

#run logistic regression
model = glm(outcome ~ elo.diff, data = train, family = binomial, na.action = na.omit)

# Parameter estimates and additional information
summary(model)

# apply prediction curve
curve(predict(model,data.frame(elo.diff=x),type="resp"),add=TRUE, col="black") 

#apply points
points(train$elo.diff,fitted(model),pch=20, col='black')


######## TESTING PERFORMANCE #######

#test accuracy on test set
fitted.results <- predict(model,newdata=subset(test,type='response'))
fitted.results <- ifelse(fitted.results > 0.5,1,0)
misClasificError <- mean(fitted.results != test$outcome)
print(paste('Accuracy',1-misClasificError))


library(ROCR)
p <- predict(model, newdata=subset(test, type="response"))
pr <- prediction(p, mydata$outcome)
prf <- performance(pr, measure = "tpr", x.measure = "fpr")
windows(title="ROC curve")
plot(prf, cex.lab=1.5)

auc <- performance(pr, measure = "auc")
auc <- auc@y.values[[1]]
auc

###### ADDITIONAL ANALYSIS #########

#Pseudo R2
library(rcompanion)
nagelkerke(model)

# Wald test
# Note that testing p-values for a logistic regression uses Chi-square tests.  
# This is achieved through the test=“Wald” option in Anova to test the significance of 
# each coefficient (in our case, there is ony one), and the test=“Chisq” option in anova 
# for the significance of the overall model.  
library(car)

Anova(model, type="II", test="Wald")


######### MISSCELANOIUS FUNCTIONS ########

#apply logistic regression with library
library(popbio)
logi.hist.plot(mydata$elo.diff, mydata$outcome, logi.mod = 1, type = "hist", boxp=FALSE, col="gray", xlab="ELO Difference")

----------------------------------------
# manually apply the prediction curve from the model to remove dots from inital plot
windows(title="No Dots")
x <- c(-1000:1000)
b = model$coefficients[1]  # intercept
m = model$coefficients[2] # slope

y <- exp((b + m*x)) / (1 + exp((b + m*x)))

plot(x, y, xlab="Probability of winning", ylab="Elo Difference", pch = , ylim=c(0,1), xlim=c(-1000,1000), col='black', cex.lab=1.5)

```
