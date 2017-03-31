# INFOB3OMG
We took the "[INFOB3OMG] Research methods for game technology" course at Utrecht University. Our research question was whether the Elo rating system could be used to predict the outcome of the popular multiplayer game Dota 2. The final report can be found in the repository above. This GitHub Readme.md is essentially the Supplementary Online Material and it contains additional statistical analysis that weren't included in the final report.

The report contains some [controversial](http://stats.stackexchange.com/questions/3559/which-pseudo-r2-measure-is-the-one-to-report-for-logistic-regression-cox-s) values, in particular pseudo-R^2 and p-values. Since a presentation will be given to students that might have a no background, these values made it to our report.

### Statistical Analysis
This section will be updated very soon.

### Raw data
Magnet link to raw match data in tar -zcvf (gzip) format, including a sample of what the data looks like.
If only `x` of matches is needed, use `zcat rawMatches.tar.gz | head -x > newfile.json` to decompress the first `x` number of matches 
```
magnet:?xt=urn:btih:3e862a0f2073ae76a66c4f054c2fe6c45c80ea19&dn=raw+match+data
```
If nobody is seeding, contact me: snookik@gmail.com

### Statistic Procedures in R
#### Reading and splitting the data into train and test set
``` R
#read the file
mydata = read.csv("path", sep = ";")

# split the index to 80% of the sample size
smp_size <- floor(0.80 * nrow(mydata))

# set the seed for the shuffle to make our partition reproductible
set.seed(123)
train_ind <- sample(seq_len(nrow(mydata)), size = smp_size)

# split the data using the index
train <- mydata[train_ind, ]
test <- mydata[-train_ind, ]

```

#### Fitting the model and plotting the curve
``` R
# show the active window
windows(title="Elodiff vs. outcome")

#plot the graph
plot(train$elo.diff, train$outcome, xlab="Elo difference", ylab="Probability of winning", xlim=c(-1000,1000),cex.lab=1.5)

# fit model 
model = glm(outcome ~ elo.diff, data = train, family = binomial, na.action = na.omit)

# Parameter estimates, statistical significance and other information
summary(model)

# plot prediction curve
curve(predict(model,data.frame(elo.diff=x),type="resp"),add=TRUE, col="black") 
points(train$elo.diff,fitted(model),pch=20, col='black')
```

#### Testing performance
```R
# Decision Boundary
fitted.results <- predict(model,newdata=subset(test,type='response'))
fitted.results <- ifelse(fitted.results > 0.5,1,0)
misClasificError <- mean(fitted.results != test$outcome)
print(paste('Accuracy',1-misClasificError))

# ROC
library(ROCR)
p <- predict(model, newdata=subset(test, type="response"))
pr <- prediction(p, mydata$outcome)
prf <- performance(pr, measure = "tpr", x.measure = "fpr")
windows(title="ROC curve")
plot(prf, cex.lab=1.5)

auc <- performance(pr, measure = "auc")
auc <- auc@y.values[[1]]
auc
```

#### Model Evaluation and Diagnostics
``` R
#Pseudo R^2
library(rcompanion)
nagelkerke(model)

# Wald test
# Note that testing p-values for a logistic regression uses Chi-square tests.  
# This is achieved through the test=“Wald” option in Anova to test the significance of 
# each coefficient (in our case, there is ony one), and the test=“Chisq” option in anova 
# for the significance of the overall model. 
library(car)
Anova(model, type="II", test="Wald")
```

#### Additional plotting functions

``` R
#plot logistic regression with library that will also show a histogram.
library(popbio)
logi.hist.plot(mydata$elo.diff, mydata$outcome, logi.mod = 1, type = "hist", boxp=FALSE, col="gray", xlab="ELO Difference")

----------------------------------------
# manually apply the curve from the model to remove dots from inital plot
windows(title="No Dots")
x <- c(-1000:1000)
b = model$coefficients[1]  # intercept
m = model$coefficients[2] # slope

y <- exp((b + m*x)) / (1 + exp((b + m*x)))

plot(x, y, xlab="Probability of winning", ylab="Elo Difference", pch = , ylim=c(0,1), xlim=c(-1000,1000), col='black', cex.lab=1.5)

```
